package main

import (
	"bytes"
	"encoding/binary"
	"fmt"
	"io/ioutil"
	"os"
)

const (
	numSingles = 64
	numMultis  = 64
	numDrums   = 682
	numEffects = 32

	headerSize = 8

	singleDataSize = 131
	multiDataSize  = 77
	drumDataSize   = 682
	effectDataSize = 35

	numSources = 4

	singleDataStart = headerSize
	multiDataStart  = singleDataStart + 64*singleDataSize
	drumDataStart   = multiDataStart + 64*multiDataSize
	effectDataStart = drumDataStart + 682
	eoxStart        = effectDataStart + 32*effectDataSize
)

type SingleCommon struct {
	Name string // 10 ASCII characters

}

type SingleSource struct {
	Delay int
	Wave  int
}

type Single struct {
	Common  SingleCommon
	Sources [numSources]SingleSource
}

func main() {
	fmt.Println("Hello, Kawai K4!")

	inputFileName := os.Args[1]
	fmt.Printf("Reading from input file '%s'\n", inputFileName)
	data, err := ioutil.ReadFile(inputFileName) // read the whole file into memory
	if err != nil {
		fmt.Printf("error opening %s: %s\n", inputFileName, err)
		os.Exit(1)
	}

	fmt.Printf("Length of data = %d\n", len(data))

	fmt.Println("Parsing from SysEx file")

	buf := bytes.NewReader(data)

	// Read the SysEx header
	var header [8]byte
	err = binary.Read(buf, binary.BigEndian, &header)
	if err != nil {
		fmt.Println("binary read failed: ", err)
		os.Exit(1)
	}

	// Examine the header.
	// Manufacturer list: https://www.midi.org/specifications-old/item/manufacturer-id-numbers
	if header[0] != 0xF0 {
		fmt.Println("Error: SysEx file must start with F0 (hex)")
		os.Exit(1)
	}

	if header[1] != 0x40 {
		fmt.Println("Error: Manufacturer ID for Kawai should be 40 (hex)")
		os.Exit(1)
	}

	channel := header[2]
	fmt.Printf("MIDI channel = %d\n", channel+1)

	function := header[3]
	group := header[4]
	machine := header[5]
	subStatus1 := header[6]
	subStatus2 := header[7]

	fmt.Printf("function = %xh, group = %d, machine = %d, sub status1 = %d, sub status2 = %d\n",
		function, group, machine, subStatus1, subStatus2)

	fmt.Println("Data offsets:")
	fmt.Printf("Single data = %04X\n", singleDataStart)
	fmt.Printf("Multi data = %04X\n", multiDataStart)
	fmt.Printf("Drum data = %04X\n", drumDataStart)
	fmt.Printf("Effect data = %04X\n", effectDataStart)
	fmt.Printf("EOX = %04X\n", eoxStart)

	listSinglePatches(data[singleDataStart:multiDataStart])

}

func listSinglePatches(d []byte) {
	offset := 0
	for i := 0; i < numSingles; i++ {
		name := d[offset : offset+10]
		fmt.Printf("%s\n", name)
		offset += singleDataSize
	}
}
