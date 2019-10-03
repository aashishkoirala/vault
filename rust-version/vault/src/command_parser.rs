use clap::{App, ArgMatches};

pub struct CommandParams {
    pub command: String,
    pub in_file_name: String,
    pub out_file_name: String,
    pub key_file_name: String,
    pub key_passphrase: String,
    pub use_key_passphrase: bool
}

impl CommandParams {
    pub fn new() -> CommandParams {
        CommandParams { 
            command: String::new(),
            in_file_name: String::new(),
            out_file_name: String::new(),
            key_file_name: String::new(),
            key_passphrase: String::new(),
            use_key_passphrase: false
        }
    }
}

pub fn parse_command(params: &mut CommandParams) -> Result<(), String> {
    let yaml = load_yaml!("cli.yaml");
    let matches = App::from_yaml(yaml).get_matches();
    
    match matches.subcommand_name() {
        Some(subcommand) => {
            params.command.push_str(subcommand);
            match matches.subcommand_matches(subcommand) {
                Some(matches) => {
                    match subcommand {
                        "encrypt" | "decrypt" => {
                            parse_arg("in", &mut params.in_file_name, &matches.value_of("in"))?;
                            parse_arg("out", &mut params.out_file_name, &matches.value_of("out"))?;
                            let mut params_for_key = params;
                            parse_key_args(&mut params_for_key, &matches)?;
                            return Ok(());
                        },
                        "genkey" => {
                            parse_arg("key", &mut params.key_file_name, &matches.value_of("key"))?;
                            match matches.value_of("phrase") {
                                Some(p) => {
                                    params.key_passphrase.push_str(p);
                                    params.use_key_passphrase = true;
                                },
                                _ => ()
                            };
                            return Ok(());
                        },
                        c => return Err(format!("Invalid command - {}", c))
                    }
                },
                _ => return Err("No command specified.".to_string())
            }
        },
        _ => return Err("No command specified.".to_string())
    };
}

fn parse_arg(param_name: &str, param: &mut String, arg: &Option<&str>) -> Result<(), String> {
    match arg {
        Some(s) => param.push_str(s),
        None => return Err(format!("Missing parameter - {}", param_name))
    };
    return Ok(());
}

fn parse_key_args(params: &mut CommandParams, matches: &ArgMatches) -> Result<(), String> {
    if matches.is_present("key") && matches.is_present("phrase") {
        return Err("You cannot specify both a key file and a passphrase. Please pick one or the other.".to_string());
    }
    match matches.value_of("key") {
        Some(k) => params.key_file_name.push_str(k),
        _ => match matches.value_of("phrase") {
            Some(p) => {
                params.key_passphrase.push_str(p);
                params.use_key_passphrase = true;
            },
            _ => return Err("You must specify either a key file or a key passphrase.".to_string())
        }
    };
    return Ok(());
}