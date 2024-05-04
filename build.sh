dotnet build
if [[ $? -ne 0 ]]; then exit 1; fi

arduino-cli compile --fqbn arduino:avr:micro
if [[ $? -ne 0 ]]; then exit 1; fi

arduino-cli upload -p /dev/ttyACM0 --fqbn arduino:avr:micro
