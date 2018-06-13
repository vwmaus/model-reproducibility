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
ENV GITHUB_RELEASE=v1.0

### Input data file path or link of external input data
ENV INPUT_DATA_FILE_PATH=

### Set workspace 
WORKDIR /workspace

### Set root user
USER root

### Clone GitHub repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

### Copy/download uploaded model input data to workspace directory
# No URL for model input data file provided
## No model input data file uploaded

### Create output directory
RUN mkdir -p /output

### Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

### Install unzip package
RUN apt-get install unzip

### Remove data folder
RUN rm -rf /workspace/data

### Unzip data and move it into the models' input folder (data folder) 
# "-o: overwrite", "-q: quiet", "-d destination"
# No input data file provided

### Upload/copy license (file must be called gamslice.txt)
## No licence file found or entered!
RUN curl -SL ${LICENSE_PATH} --create-dirs -o /opt/gams/gamslice.txt

# Reconfigure GAMS if user provided license
CMD ["/bin/bash", "-c", "gamsinst -a"]

### Run model during container start
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/result"]