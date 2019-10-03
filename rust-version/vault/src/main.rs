#[macro_use]
extern crate clap;

mod command_parser;
mod notifier;
mod key_handler;
mod cipher_handler;

use std::{env, process};
use command_parser::{CommandParams, parse_command};
use notifier::{write_success, write_error, handle_error};
use key_handler::{read_key, generate_key};
use cipher_handler::{encrypt, decrypt};

fn main() {
    let mut params: CommandParams = CommandParams::new();
    match parse_command(&mut params) {
        Ok(()) => (),
        Err(msg) => {
            write_error(&msg);
            println!("");
            println!("Use --help for usage instructions.");
            process::exit(1);
        }
    };

    let result = match params.command.as_ref() {
        "genkey" => generate_key(&params.key_file_name, &params.key_passphrase, params.use_key_passphrase),
        "encrypt" => encrypt(&params.in_file_name, &params.out_file_name, &params.key_file_name, &params.key_passphrase, params.use_key_passphrase),
        "decrypt" => decrypt(&params.in_file_name, &params.out_file_name, &params.key_file_name, &params.key_passphrase, params.use_key_passphrase),
        c => Err(format!("Invalid command - {}", c))
    };

    match result {
        Ok(msg) => {
            write_success(&msg);
            process::exit(0);
        },
        Err(err) => {
            write_error(&err);
            println!("");
            let mut log_file_name = env::temp_dir();
            log_file_name.push("ak-vault-errors.log");
            match log_file_name.into_os_string().into_string() {
                Ok(l) => println!("Check the log for any error details: {}", l),
                Err(_) => ()
            };
            process::exit(1);
        }
    };
}