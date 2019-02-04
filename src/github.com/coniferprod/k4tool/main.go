package main

import (
	"bytes"
	"encoding/binary"
	"fmt"
	"io/ioutil"
	"os"
)

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

}
