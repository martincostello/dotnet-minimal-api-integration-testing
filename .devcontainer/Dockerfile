FROM mcr.microsoft.com/vscode/devcontainers/universal:latest@sha256:470388ee9a70db70d66930001b4bb2dd38a8c968c6b0b241171882c855df403b

# Suppress an apt-key warning about standard out not being a terminal. Use in this script is safe.
ENV APT_KEY_DONT_WARN_ON_DANGEROUS_USAGE=DontWarn

# Install Google Chrome
RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | sudo apt-key add -
RUN echo 'deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main' | sudo tee /etc/apt/sources.list.d/google-chrome.list
RUN sudo apt-get update
RUN sudo apt-get --yes install google-chrome-stable 2>&1

# Install Firefox
RUN sudo apt-get --yes install firefox 2>&1
