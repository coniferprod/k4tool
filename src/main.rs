use std::{fs, path::PathBuf};
use std::io::Read;

use clap::{Parser, Subcommand};
use ksynth::k4::WaveNumber;
use ksynth::k4::wave::Wave;
use ksynth::k4::bank::Bank;
use ksynth::{Ranged, SystemExclusiveData};

#[derive(Parser, Debug)]
#[command(version, about, long_about = None)]
struct Args {
    #[command(subcommand)]
    command: Option<Commands>,
}

#[derive(Subcommand, Debug, Clone)]
enum Commands {
    /// display the wave list
    Wave,

    /// list the patches in a bank
    List {
        /// filename
        #[arg(short, long)]
        filename: PathBuf,

        /// output format
        #[arg(short, long)]
        output: String,
    }
}

fn main() {
    let args = Args::parse();

    match &args.command {
        Some(Commands::Wave) => print_wave_list(),
        Some(Commands::List { filename, output }) => {
            if output == "text" {
                print_text_patch_list(&filename);
            } else if output == "html" {
                print_html_patch_list(&filename);
            }
        }
        None => {}
    }
}

fn read_file(name: &PathBuf) -> Option<Vec<u8>> {
    match fs::File::open(&name) {
        Ok(mut f) => {
            let mut buffer = Vec::new();
            match f.read_to_end(&mut buffer) {
                Ok(_) => Some(buffer),
                Err(_) => None
            }
        },
        Err(_) => {
            eprintln!("Unable to open file {}", &name.display());
            None
        }
    }
}

fn print_text_patch_list(filename: &PathBuf) {
    if let Some(buffer) = read_file(filename) {
        if buffer.len() != Bank::data_size() {
            eprintln!("Not a bank file");
            return;
        }

        match Bank::from_bytes(&buffer) {
            Ok(_) => {
                println!("Parsed a bank");

            },
            Err(e) => {
                eprintln!("Bank parse failed, error: {}", e);
            }
        }

    } else {
        eprintln!("Error reading bank file");
    }
}

fn print_html_patch_list(filename: &PathBuf) {
    println!("HTML patch list");
}

fn print_wave_list() {
    for i in 1..=256 {
        let wave = Wave { number: WaveNumber::new(i) };
        println!("{}", wave);
    }
}