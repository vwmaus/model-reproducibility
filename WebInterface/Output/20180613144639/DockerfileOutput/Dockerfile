### Get base image
FROM iiasa/gams:latest

### Set maintainer
MAINTAINER "Victor Maus" maus@iiasa.ac.at

### Program license path
ENV LICENSE_PATH=

### Get GitHub repository from web interface
ENV GITHUB_USER=ptrkrnstnr

### Get GitHub repository from web interface 
ENV GITHUB_REPO=transport-model

### Get model version from web interface 
ENV GITHUB_RELEASE=v3.0

### Input data file path or link of external input data
ENV INPUT_DATA_FILE_PATH=

### Set workspace 
WORKDIR /workspace

### Set root user
USER root

### Clone GitHub repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

### Copy/download uploaded model input data to workspace directory
# No model input data file uploaded

### Create output directory
RUN mkdir -p /output

### Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

### Unzip data and move it into the models' input folder (data folder) 
# "-o: overwrite", "-q: quiet", "-d destination"
# No model input data file uploaded

### Upload/copy license (file must be called gamslice.txt)
# No licence file found or entered!

# Reconfigure GAMS if user provided license
# No licence file found or entered!

### Run model during container start
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/result"]
