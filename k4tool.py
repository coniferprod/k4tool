import sys
import os
import argparse

import waveform
import message

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
        if args.number is None:
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
    elif args.command_name == 'identify':
        #print(args)
        data = args.filename.read()
        #print(len(data), type(data))

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
    elif args.command_name == 'list':
        #print('Listing contents of ', args.filename)
        data = args.filename.read()
        #print(len(data), type(data))

        msg = message.Message(data)
        header = message.Header(msg.payload)
        ident = header.identify()
        cardinality = ident['cardinality']
        if cardinality != message.Cardinality.BLOCK:
            print('Not listing single patch')
        else:
            print('Listing would appear here')

