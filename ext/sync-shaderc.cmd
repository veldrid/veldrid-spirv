@setlocal
@echo off

python %~dp0update_shaderc_sources.py --dir %~dp0shaderc --file %~dp0known_good.json
