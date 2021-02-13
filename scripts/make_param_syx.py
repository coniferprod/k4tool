#!/usr/bin/env python3

import sys

if len(sys.argv) < 6:
    print('usage: make_param_syx.py patch_type channel param_num param_target param_value')
    sys.exit(-1)

patch_type = sys.argv[1]  # single, drum, effect

channel = int(sys.argv[2])  # MIDI channel 1...16 (will use 0...15)
if channel < 1 or channel > 16:
    print(f"bad MIDI channel: {channel}")
    sys.exit(-1)
else:
    channel -= 1 # use 0...15

param_num = int(sys.argv[3])  # parameter number (single = 0...69, drum = 70...81, effect = 82...88)
if patch_type == 'single':
    if param_num < 0 or param_num > 69:
        print(f'parameter {param_num} out of range, must be 0...69')
        sys.exit(-1)
elif patch_type == 'drum':
    if param_num < 70 or param_num > 81:
        print(f'parameter {param_num} out of range, must be 70...81')
        sys.exit(-1)
elif patch_type == 'effect':
    if param_num < 82 or param_num > 88:
        print(f'parameter {param_num} out of range, must be 82...88')
        sys.exit(-1)
else:
    print('bad patch type:', patch_type)
    sys.exit(-1)

param_target = int(sys.argv[4])  # parameter target (single = 0...3 for source, drum = 0...60 for key, effect = 0...7 for submix/output ch)
if patch_type == 'single':
    if param_target < 0 or param_target > 3:
        print(f'parameter {param_target} out of range, must be 0...3')
        sys.exit(-1)
if patch_type == 'drum':
    if param_target < 0 or param_target > 60:
        print(f'parameter {param_target} out of range, must be 0...60')
        sys.exit(-1)
elif patch_type == 'effect':
    if param_target < 0 or param_target > 7:
        print(f'parameter {param_target} out of range, must be 0...7')
        sys.exit(-1)

param_value = int(sys.argv[5])

print(patch_type, channel, param_num, param_target, param_value)

PARAM_SEND = 0x10
SYNTH_GROUP = 0x00
MACHINE_ID = 0x04

msg_bytes = [0xf0, 0x40, channel, PARAM_SEND, SYNTH_GROUP, MACHINE_ID,
    param_num, param_target, param_value, 0xf7]
print(bytearray(msg_bytes).hex(' '))
