import argparse
import pathlib

import waveform
from message import (Message, Header, Kind, Cardinality, Locality, Function,
                     HEADER_SIZE, SINGLE_SIZE, SINGLE_COUNT, 
                     MULTI_SIZE, MULTI_COUNT, DRUM_SIZE, NAME_LENGTH,
                     PATCHES_PER_BANK)
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
        if 1 <= args.number < len(waveform.all_waveforms):
            print(f'{waveform.all_waveforms[args.number]}')
        else:
            print(f'Bad wave number: {args.number}')

def do_info(data):
    msg = Message.from_bytes(data)
    print(f'Manufacturer: {msg.manufacturer.name}')

    print(f'Payload: {len(msg.payload)} bytes')
    header = Header.from_bytes(msg.payload)
    ident = header.identify()
    #print(ident)

    output = ''
    cardinality = ident['cardinality']
    if cardinality == Cardinality.ONE:
        output += 'One'
    elif cardinality == Cardinality.BLOCK:
        output += 'Block'
    output += ' / '
    kind = ident['kind']
    if kind == Kind.ALL:
        output += 'All'
    elif kind == Kind.SINGLE:
        output += 'Single'
    elif kind == Kind.MULTI:
        output += 'Multi'
    output += ' / '
    locality = ident['locality']
    if locality == Locality.INTERNAL:
        output += 'INT'
    elif locality == Locality.EXTERNAL:
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
    msg = Message.from_bytes(data)
    header = Header.from_bytes(msg.payload)
    ident = header.identify()
    cardinality = ident['cardinality']
    if cardinality != Cardinality.BLOCK:
        print('Not listing single patch')
        return

    # Patch data is between header at the start
    # and checksum + 0xF7 at the end
    patch_data = msg.payload[HEADER_SIZE:-2]

    single_start = 0
    multi_start = SINGLE_COUNT * SINGLE_SIZE
    drum_start = multi_start + MULTI_COUNT * MULTI_SIZE
    effect_start = drum_start + DRUM_SIZE
    #print(f'Singles = {single_start}')
    #print(f'Multis  = {multi_start}')
    #print(f'Drum    = {drum_start}')
    #print(f'Effects = {effect_start}')

    single_names = []
    offset = single_start
    for _ in range(0, SINGLE_COUNT):
        name_data = patch_data[offset : offset + NAME_LENGTH]
        name = name_data.decode('ASCII')
        #print(f'offset = {offset}')
        single_names.append(name)
        offset += SINGLE_SIZE

    print('Single Patches'.upper())

    for (index, name) in enumerate(single_names):
        print(f'{patch_label(index, PATCHES_PER_BANK)}: {name}')

    multi_names = []
    for _ in range(0, MULTI_COUNT):
        name_data = patch_data[offset : offset + NAME_LENGTH]
        name = name_data.decode('ASCII')
        #print(f'offset = {offset}')
        multi_names.append(name)
        offset += MULTI_SIZE

    print()
    print('Multi Patches'.upper())
    for (index, name) in enumerate(multi_names):
        print(f'{patch_label(index, PATCHES_PER_BANK)}: {name}')

def do_extract(bank_name, data, kind, patch_label):
    # Get patch number from 0 .. 63 based on the label
    number = patch_number(patch_label)
    if number is None:
        print(f'Invalid patch number: "{patch_label}"')
        return
    
    if not kind in ['single', 'multi', 'drum', 'effect']:
        print(f'Invalid patch kind: {kind}')
        return
    
    msg = Message.from_bytes(data)
    header = Header.from_bytes(msg.payload)    
    patch_data = msg.payload[HEADER_SIZE:-2]
    start = 0
    size = 0
    single_start = 0
    multi_start = SINGLE_COUNT * SINGLE_SIZE
    drum_start = multi_start + MULTI_COUNT * MULTI_SIZE
    effect_start = drum_start + DRUM_SIZE
    if kind == 'single':
        start = single_start
        size = SINGLE_SIZE
    elif kind == 'multi':
        start = multi_start
        size = MULTI_SIZE
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
    out_name = bank_name + '_'
    out_name += 'S' if kind == 'single' else 'M'
    out_name += 'I' if header.locality == Locality.INTERNAL else 'E'
    out_name += f'{patch_label}.syx'

    #print(f'kind = {kind}. label = {patch_label} ({number})')
    #print(f'data = {len(payload)} bytes, name = "{out_name}"')

    out_header = Header(header.channel,
                        Function.ONE_PATCH_DATA_DUMP,
                        substatus1=0x00 if header.locality == Locality.INTERNAL else 0x02,
                        substatus2=number if kind == 'single' else number + 64)
    # Sub status 2: 0~63 = SINGLE A-1 ~ D-16, 64~127 = MULTI A-1 ~ D-16

    payload = bytearray()
    payload.extend(bytes(out_header))
    payload.extend(patch_data[start:start+size])

    out_data = bytes(Message(KAWAI, payload))

    #print(f'Final output: {len(out_data)} bytes')
    try:
        with open(out_name, 'wb') as out_file:
            out_file.write(out_data)
    except (IOError, OSError):
        print('Error extracting patch')

    #print(out_data.hex(' '))

def do_init(kind, patch_label):
    # Get patch number from 0 .. 63 based on the label
    number = patch_number(patch_label)
    if number is None:
        print(f'Invalid patch number: "{patch_label}"')
        return
    
    if not kind in ['single', 'multi', 'drum', 'effect']:
        print(f'Invalid patch kind: {kind}')
        return
    
    locality = Locality.INTERNAL
    out_header = Header(0,  # use MIDI channel 1
                    Function.ONE_PATCH_DATA_DUMP,
                    substatus1=0x00 if locality == Locality.INTERNAL else 0x02,
                    substatus2=number if kind == 'single' else number + 64)
    payload = bytearray()
    payload.extend(bytes(out_header))

    size = SINGLE_SIZE if kind == 'single' else MULTI_SIZE

    payload.extend(bytes(size)) # just the right amount of zero bytes for now

    out_data = bytes(Message(KAWAI, payload))

    out_name = 'INIT'
    out_name += 'S' if kind == 'single' else 'M'
    out_name += 'I' if locality == Locality.INTERNAL else 'E'
    out_name += f'{patch_label}.syx'.upper()
    try:
        with open(out_name, 'wb') as out_file:
            out_file.write(out_data)
    except (IOError, OSError):
        print('Error writing init patch')


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
    extract_parser.add_argument("-k", "--kind", type=str, default='single')
    extract_parser.add_argument("-n", "--number", type=str, default='a1')

    init_parser = subparsers.add_parser('init')
    init_parser.add_argument("-k", "--kind", type=str, default='single')
    init_parser.add_argument("-n", "--number", type=str, default='a1')

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
    elif args.command_name == 'init':
        do_init(args.kind, args.number)
