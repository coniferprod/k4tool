import sys
import os
import argparse

import waveform
import message

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

def do_identify(data):
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


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Kawai K4 sound patch utility')
    subparsers = parser.add_subparsers(dest='command_name')

    wave_parser = subparsers.add_parser('wave')
    wave_parser.add_argument("-n", "--number", type=int, default=None)

    identify_parser = subparsers.add_parser('identify')
    identify_parser.add_argument('filename', type=argparse.FileType('rb'))

    list_parser = subparsers.add_parser('list')
    list_parser.add_argument('filename', type=argparse.FileType('rb'))

    args = parser.parse_args()

    if args.command_name == 'wave':
        do_wave(args.number)
    elif args.command_name == 'identify':
        do_identify(args.filename.read())
    elif args.command_name == 'list':
        do_list(args.filename.read())
