# Send System Exclusive message in hex format to MIDI port
# $1 is the device name as returned by `sendmidi list`
# $2 is the message in the format "f0 40 00 20 ... f7"
# Usage example: bash sendsyx.sh "Some MIDI" "$(./make_param_syx.py single 1 0 0 10)"
sendmidi dev $1 hex syx $2
