from enum import Enum, IntEnum

from manufacturer import Manufacturer

INITIATOR = 0xf0
TERMINATOR = 0xf7

class Message:
    def __init__(self, data: bytes):
        if len(data) < 4:
            raise ValueError('Not enough data for a valid MIDI System Exclusive message')
        if data[0] != INITIATOR and data[-1] != TERMINATOR:
            raise ValueError('Not a valid MIDI System Exclusive message')

        if data[1] in [0x7e, 0x7f]:
            raise ValueError('Universal System Exclusive messages are not supported')

        if data[1] == 0x00:  # extended manufacturer
            self.manufacturer = Manufacturer(data[1:4])
            self.payload = data[4:-1]
        else: # standard one-byte manufacturer
            self.manufacturer = Manufacturer(data[1:2])
            self.payload = data[2:-1]

class Function(IntEnum):
    ONE_PATCH_DUMP_REQUEST = 0x00,
    BLOCK_PATCH_DUMP_REQUEST = 0x01
    ALL_PATCH_DUMP_REQUEST = 0x02
    PARAMETER_SEND = 0x10
    ONE_PATCH_DATA_DUMP = 0x20
    BLOCK_PATCH_DATA_DUMP = 0x21
    ALL_PATCH_DATA_DUMP = 0x22
    EDIT_BUFFER_DUMP = 0x23
    PROGRAM_CHANGE = 0x30
    WRITE_COMPLETE = 0x40
    WRITE_ERROR = 0x41
    WRITE_ERROR_PROTECT = 0x42
    WRITE_ERROR_NO_CARD = 0x43

class Locality(Enum):
    INTERNAL = 1
    EXTERNAL = 2

class Cardinality(Enum):
    ONE = 1
    BLOCK = 2

class Kind(Enum):
    SINGLE = 1
    MULTI = 2
    DRUM = 3
    EFFECT = 4
    ALL = 5

HEADER_SIZE = 6 # length of header in bytes
SINGLE_COUNT = 64
MULTI_COUNT = 64
EFFECT_COUNT = 32
SINGLE_SIZE = 131
MULTI_SIZE = 77
EFFECT_SIZE = 35
DRUM_SIZE = 682
NAME_LENGTH = 10
PATCHES_PER_BANK = 16

class Header:
    def __init__(self, data: bytes):
        self.channel = data[0]
        self.function = data[1]
        # data[2]: Group = 0x00
        # data[3]: machine = 0x04
        self.substatus1 = data[4]
        self.substatus2 = data[5]

    @property
    def locality(self):
        if self.substatus1 in [0x00, 0x01]:
            return Locality.INTERNAL
        elif self.substatus1 in [0x02, 0x03]:
            return Locality.EXTERNAL

    @property
    def cardinality(self):
        if self.function == 0x20:
            return Cardinality.ONE
        elif self.function == 0x21:
            return Cardinality.BLOCK
        elif self.function == 0x22:
            return Cardinality.ALL

    def identify(self):
        # Default values
        kind = Kind.SINGLE
        locality = Locality.INTERNAL
        cardinality = Cardinality.ONE
        patch_number = None
        match (self.function, self.substatus1, self.substatus2):
            case (Function.ONE_PATCH_DATA_DUMP, 0x00, number) if 0 <= number <= 63:
                patch_number = number
            case (Function.ONE_PATCH_DATA_DUMP, 0x00, number) if 64 <= number <= 127:
                kind = Kind.MULTI
                patch_number = number
            case (Function.ONE_PATCH_DATA_DUMP, 0x02, number) if 0 <= number <= 63:
                cardinality = Cardinality.ONE
                patch_number = number
                locality = Locality.EXTERNAL
            case (Function.ALL_PATCH_DATA_DUMP, 0x00, 0x00):
                kind = Kind.ALL
                cardinality = Cardinality.BLOCK
            case (Function.ALL_PATCH_DATA_DUMP, 0x02, 0x00):
                kind = Kind.ALL
                cardinality = Cardinality.BLOCK
                locality = Locality.EXTERNAL

        return {'kind': kind, 'locality': locality, 'cardinality': cardinality,
                'patch_number': patch_number}
