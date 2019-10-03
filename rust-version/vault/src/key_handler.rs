extern crate rand;

use std::fs::File;
use std::io::{Read, Write};
use std::iter::repeat;
use rand::{OsRng, Rng};
use crate::notifier::handle_error;

pub fn generate_key(key_file_name: &String, key_passphrase: &String, use_key_passphrase: bool) -> Result<String, String> {
    println!("Generating key and writing to file {}...", &key_file_name);
    let mut rng = match OsRng::new() {
        Ok(rng) => rng,
        Err(err) => return handle_error(err, "Cannot get random number generator from OS")
    };
    let mut key: Vec<u8> = repeat(0u8).take(16).collect();
    rng.fill_bytes(&mut key);
    let mut key_file = match File::create(key_file_name) {
        Ok(file) => file,
        Err(err) => return handle_error(err, &format!("Cannot open file {} for writing", key_file_name))
    };
    match key_file.write_all(&key) {
        Ok(()) => return Ok("Key generated.".to_string()),
        Err(err) => return handle_error(err, &format!("Could not write key to file {}", key_file_name))
    };
}

pub fn read_key(key_file_name: &String, out_key: &mut Vec<u8>) -> Result<(), String> {
    let mut key_file = match File::open(key_file_name) {
        Ok(file) => file,
        Err(err) => return handle_error(err, &format!("Cannot open key file {}", key_file_name))
    };
    let key_length = match key_file.read_to_end(out_key) {
        Ok(len) => len,
        Err(err) => return handle_error(err, &format!("Cannot read key from file {}", key_file_name))
    };
    match key_length {
        16 => return Ok(()),
        l => return Err(format!("Invalid key length: {}, must be 16 bytes", l))
    };
}