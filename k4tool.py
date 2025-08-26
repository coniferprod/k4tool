import sys
import os
import argparse
import pathlib

import waveform
import message
from manufacturer import KAWAI

def do_wave(number):
    if number is None:
        # Show the names of all waves
        numbers = waveform.all_waveforms.keys()
        keys = sorted(list(numbers))
        for key in keys:
            print(f'{key:3} {waveform.all_waveforms[key]}')
    else:
        # Show just the wave with the specified number
        if args.number in waveform.all_waveforms.keys():
            print(f'{args.number:3} {waveform.all_waveforms[args.number]}')
        else:
            print(f'Bad wave number: {args.number}')

def do_info(data):
    msg = message.Message(data)
    print(f'Manufacturer: {msg.manufacturer.name}')

    print(f'Payload: {len(msg.payload)} bytes')
    header = message.Header(msg.payload)
    ident = header.identify()
    #print(ident)

    output = ''
    cardinality = ident['cardinality']
    if cardinality == message.Cardinality.ONE:
        output += 'One'
    elif cardinality == message.Cardinality.BLOCK:
        output += 'Block'
    output += ' / '
    kind = ident['kind']
    if kind == message.Kind.ALL:
        output += 'All'
    elif kind == message.Kind.SINGLE:
        output += 'Single'
    elif kind == message.Kind.MULTI:
        output += 'Multi'
    output += ' / '
    locality = ident['locality']
    if locality == message.Locality.INTERNAL:
        output += 'INT'
    elif locality == message.Locality.EXTERNAL:
        output += 'EXT'
    output += ' '
    number = ident['patch_number']
    if number is not None:
        output += f'{number}'

    print(f'Identification: {output}')

def patch_label(patch_number, patch_count):
    bank_index = patch_number // patch_count
    bank_letter = 'ABCD'[bank_index]
    patch_index = (patch_number % patch_count) + 1
    return f'{bank_letter}-{patch_index}'

def patch_number(patch_label):
    bank_letters = ['a', 'b', 'c', 'd']
    letter = patch_label[0].lower()
    if not letter in bank_letters:
        return None
    letter_index = bank_letters.index(letter)
    number = int(patch_label[1:])
    return letter_index * 16 + number

def do_list(data):
    msg = message.Message(data)
    header = message.Header(msg.payload)
    ident = header.identify()
    cardinality = ident['cardinality']
    if cardinality != message.Cardinality.BLOCK:
        print('Not listing single patch')
        return

    # Patch data is between header at the start
    # and checksum + 0xF7 at the end
    patch_data = msg.payload[message.HEADER_SIZE:-2]

    single_start = 0
    multi_start = message.SINGLE_COUNT * message.SINGLE_SIZE
    drum_start = multi_start + message.MULTI_COUNT * message.MULTI_SIZE
    effect_start = drum_start + message.DRUM_SIZE
    #print(f'Singles = {single_start}')
    #print(f'Multis  = {multi_start}')
    #print(f'Drum    = {drum_start}')
    #print(f'Effects = {effect_start}')

    single_names = []
    offset = single_start
    for _ in range(0, message.SINGLE_COUNT):
        name_data = patch_data[offset : offset + message.NAME_LENGTH]
        name = name_data.decode('ASCII')
        #print(f'offset = {offset}')
        single_names.append(name)
        offset += message.SINGLE_SIZE

    print('Single Patches'.upper())

    for (index, name) in enumerate(single_names):
        print(f'{patch_label(index, message.PATCHES_PER_BANK)}: {name}')

    multi_names = []
    for _ in range(0, message.MULTI_COUNT):
        name_data = patch_data[offset : offset + message.NAME_LENGTH]
        name = name_data.decode('ASCII')
        #print(f'offset = {offset}')
        multi_names.append(name)
        offset += message.MULTI_SIZE

    print()
    print('Multi Patches'.upper())
    for (index, name) in enumerate(multi_names):
        print(f'{patch_label(index, message.PATCHES_PER_BANK)}: {name}')

def do_extract(bank_name, data, kind, patch_label):
    # Get patch number from 0 .. 63 based on the label
    number = patch_number(patch_label)
    if number is None:
        print(f'Invalid patch number: "{patch_label}"')
        return
    
    if not kind in ['single', 'multi', 'drum', 'effect']:
        print(f'Invalid patch kind: {kind}')
        return
    
    msg = message.Message(data)
    header = message.Header(msg.payload)    
    patch_data = msg.payload[message.HEADER_SIZE:-2]
    start = 0
    size = 0
    single_start = 0
    multi_start = message.SINGLE_COUNT * message.SINGLE_SIZE
    drum_start = multi_start + message.MULTI_COUNT * message.MULTI_SIZE
    effect_start = drum_start + message.DRUM_SIZE
    if kind == 'single':
        start = single_start
        size = message.SINGLE_SIZE
    elif kind == 'multi':
        start = multi_start
        size = message.MULTI_SIZE
    #elif kind == 'drum':
    #    start = drum_start
    #    size = message.DRUM_SIZE
    #elif kind == 'effect':
    #    start = effect_start
    #    size = message.EFFECT_SIZE
    else:
        print('Only single and multi patches can be extracted')
        return

    # Calculate offset from block start based on patch number
    start += number * size
    payload = patch_data[start:start+size]
    out_name = bank_name + '_'
    out_name += 'S' if kind == 'single' else 'M'
    out_name += 'I' if header.locality == message.Locality.INTERNAL else 'E'
    out_name += f'{patch_label}.syx'

    #print(f'kind = {kind}. label = {patch_label} ({number})')
    #print(f'data = {len(payload)} bytes, name = "{out_name}"')

    out_header = message.Header(bytes(message.HEADER_SIZE))
    out_header.channel = header.channel
    out_header.function = message.Function.ONE_PATCH_DATA_DUMP
    out_header.group = 0x00
    out_header.machine = 0x04
    out_header.substatus1 = 0x01 # internal

    # Sub status 2: 0~63 = SINGLE A-1 ~ D-16, 64~127 = MULTI A-1 ~ D-16
    out_header.substatus2 = number if kind == 'single' else number + 64

    out_data = bytearray()
    out_data.append(0xf0)
    out_data.extend(bytes(KAWAI))
    out_data.extend(bytes(out_header))
    out_data.extend(payload)
    out_data.append(0xf7)

    #print(f'Final output: {len(out_data)} bytes')
    try:
        with open(out_name, 'wb') as out_file:
            out_file.write(out_data)
    except (IOError, OSError):
        print('Error extracting patch')

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Kawai K4 sound patch utility')
    subparsers = parser.add_subparsers(dest='command_name')

    wave_parser = subparsers.add_parser('wave')
    wave_parser.add_argument("-n", "--number", type=int, default=None)

    info_parser = subparsers.add_parser('info')
    info_parser.add_argument('filename', type=argparse.FileType('rb'))

    list_parser = subparsers.add_parser('list')
    list_parser.add_argument('filename', type=argparse.FileType('rb'))

    extract_parser = subparsers.add_parser('extract')
    extract_parser.add_argument('filename', type=argparse.FileType('rb'))
    extract_parser.add_argument("-n", "--number", type=str, default='a1')
    extract_parser.add_argument("-k", "--kind", type=str, default='single')

    args = parser.parse_args()

    if args.command_name == 'wave':
        do_wave(args.number)
    elif args.command_name == 'info':
        do_info(args.filename.read())
    elif args.command_name == 'list':
        do_list(args.filename.read())
    elif args.command_name == 'extract':
        # Get the name of the bank file without extension:
        bank_name = pathlib.Path(args.filename.name).stem
        do_extract(bank_name, args.filename.read(), args.kind, args.number)
