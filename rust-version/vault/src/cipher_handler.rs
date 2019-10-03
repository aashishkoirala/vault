extern crate rand;
extern crate crypto;

use std::fs::File;
use std::io::{Read, Write};
use std::iter::repeat;
use rand::{OsRng, Rng};
use crypto::aes::{self, KeySize};
use crate::notifier::{write_success, write_error, handle_error};
use crate::key_handler::{read_key, generate_key};

pub fn encrypt(in_file_name: &String, out_file_name: &String, key_file_name: &String, key_passphrase: &String, use_key_passphrase: bool) -> Result<String, String> {
    println!("Encrypting {} to {} using key in {}...", &key_file_name, &in_file_name, &out_file_name);
    let mut key: Vec<u8> = Vec::new();
    read_key(key_file_name, &mut key)?;
    let mut in_file = match File::open(in_file_name) {
        Ok(file) => file,
        Err(err) => return handle_error(err, &format!("Cannot open file {}", in_file_name))
    };
    let mut in_file_data: Vec<u8> = Vec::new();
    let in_file_length = match in_file.read_to_end(&mut in_file_data) {
        Ok(len) => len,
        Err(err) => return handle_error(err, &format!("Cannot read from file {}", in_file_name))
    };
    let mut rng = match OsRng::new() {
        Ok(rng) => rng,
        Err(err) => return handle_error(err, "Cannot get random number generator from OS")
    };
    let mut iv: Vec<u8> = repeat(0u8).take(16).collect();
    rng.fill_bytes(&mut iv);
    let mut cipher = aes::ctr(KeySize::KeySize256, &key, &iv);
    let mut out_file_data: Vec<u8> = repeat(0u8).take(in_file_length).collect();    
    cipher.process(&in_file_data, &mut out_file_data);
    let mut out_file = match File::create(out_file_name) {
        Ok(file) => file,
        Err(err) => return handle_error(err, &format!("Cannot create file {}", out_file_name))
    };
    match out_file.write_all(&iv) {
        Ok(()) => (),
        Err(err) => return handle_error(err, &format!("Cannot write to file {}", out_file_name))
    };
    match out_file.write_all(&out_file_data) {
        Ok(()) => return Ok("Encryption complete.".to_string()),
        Err(err) => return handle_error(err, &format!("Cannot write to file {}", out_file_name))
    };
}

pub fn decrypt(in_file_name: &String, out_file_name: &String, key_file_name: &String, key_passphrase: &String, use_key_passphrase: bool) -> Result<String, String> {    
    println!("Decrypting {} to {} using key in {}...", &key_file_name, &in_file_name, &out_file_name);
    let mut key: Vec<u8> = Vec::new();
    read_key(key_file_name, &mut key)?;
    let mut in_file = match File::open(in_file_name) {
        Ok(file) => file,
        Err(err) => return handle_error(err, &format!("Cannot open file {}", in_file_name))
    };
    let mut iv: Vec<u8> = repeat(0u8).take(16).collect();
    match in_file.read(&mut iv) {
        Ok(16) => (),
        Err(err) => return handle_error(err, &format!("Cannot read IV from file {}", in_file_name)),
        Ok(n) => return Err(format!("Expected 16 bytes of IV, got {} bytes", n))
    };
    let mut in_file_data: Vec<u8> = Vec::new();
    let in_file_length = match in_file.read_to_end(&mut in_file_data) {
        Ok(l) => l,
        Err(err) => return handle_error(err, &format!("Cannot read data from file {}", in_file_name))
    };    
    let mut cipher = aes::ctr(KeySize::KeySize256, &key, &iv);
    let mut out_file_data: Vec<u8> = repeat(0u8).take(in_file_length).collect();
    cipher.process(&in_file_data, &mut out_file_data);
    let mut out_file = match File::create(out_file_name) {
        Ok(file) => file,
        Err(err) => return handle_error(err, &format!("Cannot open file {} for writing", out_file_name))
    };
    match out_file.write_all(&out_file_data) {
        Ok(()) => return Ok("Decryption complete.".to_string()),
        Err(err) => return handle_error(err, &format!("Cannot write to file {}", out_file_name))
    };
}