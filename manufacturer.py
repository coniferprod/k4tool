DEVELOPMENT = 0x7D

MANUFACTURERS = {
    (0x01,): 'Sequential Circuits',
    (0x00, 0x00, 0x01): 'Time/Warner Interactive',
    (0x00, 0x00, 0x0E): 'Alesis Studio Electronics',
    (0x00, 0x20, 0x29): 'Focusrite/Novation',
    (0x40,): 'Kawai Musical Instruments MFG. CO. Ltd',
    (0x41,): 'Roland Corporation',
    (0x42,): 'Korg Inc.',
    (0x43,): 'Yamaha Corporation',
    (DEVELOPMENT,): 'Development/Non-commercial'
}

def find_manufacturer(identifier):
    if identifier in MANUFACTURERS:
        return MANUFACTURERS[identifier]
    else:
        return None

class Manufacturer:
    def __init__(self, data: bytes):
        self.identifier = (data[0],)
        if self.identifier[0] == 0x00:
            self.identifier = (data[0], data[1], data[2])

    def __str__(self) -> str:
        s = '{} ('.format(self.name)
        for i in range(len(self.identifier)):
            s += '{0:02X}H'.format(self.identifier[i])
            if i < len(self.identifier) - 1:
                s += ' '
        s += ')'
        return s

    @property
    def name(self) -> str:
        if self.identifier in MANUFACTURERS:
            return MANUFACTURERS[self.identifier]
        else:
            return '*unknown*'
