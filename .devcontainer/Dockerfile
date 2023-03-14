FROM mcr.microsoft.com/devcontainers/dotnet:0-6.0-bullseye

RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get autoremove -y && \
    apt-get clean -y

#install k3d, dapr cli, kubectl
RUN wget -q -O - https://raw.githubusercontent.com/k3d-io/k3d/main/install.sh | bash
RUN wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash

RUN apt-get install -y ca-certificates curl
RUN mkdir -p /etc/apt/keyrings
RUN sudo curl -fsSLo /etc/apt/keyrings/kubernetes-archive-keyring.gpg https://packages.cloud.google.com/apt/doc/apt-key.gpg
RUN echo "deb [signed-by=/etc/apt/keyrings/kubernetes-archive-keyring.gpg] https://apt.kubernetes.io/ kubernetes-xenial main" | sudo tee /etc/apt/sources.list.d/kubernetes.list
RUN apt-get update && apt-get install -y kubectl uuid-runtime

ARG USERNAME=vscode
RUN usermod -s /bin/zsh $USERNAME
USER $USERNAME

RUN . $NVM_DIR/nvm.sh; nvm install --lts; npm install -g parcel

# set up user tools
RUN curl -sS https://webinstall.dev/k9s | bash
RUN curl -fsSL https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | /bin/bash
RUN dotnet tool install -g dotnet-reportgenerator-globaltool

# set up oh-my-zsh
RUN sh -c "$(wget -O- https://raw.githubusercontent.com/robbyrussell/oh-my-zsh/master/tools/install.sh)" "" --unattended &> /dev/null
ENV ENV="/home/$USERNAME/.ashrc" \
    ZSH=/home/$USERNAME/.oh-my-zsh \
    EDITOR=vi \
    LANG=en_US.UTF-8
RUN printf 'ZSH_THEME="robbyrussell"\nENABLE_CORRECTION="false"\nplugins=(git copyfile extract colorize dotenv encode64 golang)\nsource $ZSH/oh-my-zsh.sh\nexport PATH=$PATH:~/.local/bin' > "/home/$USERNAME/.zshrc"
RUN echo "exec `which zsh`" > "/home/$USERNAME/.ashrc"
