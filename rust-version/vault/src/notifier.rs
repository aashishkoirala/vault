extern crate term;

use std::env;
use std::fs::OpenOptions;
use std::io::{Write, Error};
use term::color;

pub fn write_success(msg: &str) {
    write_msg(color::GREEN, msg, false);
}

pub fn write_error(msg: &str) {
    write_msg(color::RED, msg, true);
}

pub fn handle_error<T>(err: Error, msg: &str) -> Result<T, String> {
    write_error_log(&err.to_string());
    return Err(msg.to_string());
}

pub fn write_msg(color: u32, msg: &str, is_error: bool) {    
    match is_error {
        true => {
            let mut t = term::stderr().unwrap();
            t.fg(color).unwrap();
            writeln!(t, "{}", msg).unwrap();
            t.reset().unwrap();
        },
        false => {
            let mut t = term::stdout().unwrap();
            t.fg(color).unwrap();
            writeln!(t, "{}", msg).unwrap();
            t.reset().unwrap();
        }
    };    
}

fn write_error_log(msg: &str) {
    let mut log_file_name = env::temp_dir();
    log_file_name.push("ak-vault-errors.log");
    let log_file = OpenOptions::new()
        .create(true)
        .append(true)
        .open(log_file_name)
        .expect("Cannot open log file for writing.");
    writeln!(&log_file, "{}", msg).expect("Cannot write to log.");
}