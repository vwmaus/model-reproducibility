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
ENV INPUT_DATA_FILE_PATH=http://webinterface/uploads/model_input_data/modelInputData.zip

### Set workspace 
WORKDIR /workspace

### Set root user
USER root

### Clone GitHub repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

### Copy/download uploaded model input data to workspace directory
RUN curl -SL ${INPUT_DATA_FILE_PATH} --create-dirs -o /workspace/data.zip
#COPY ${INPUT_DATA_FILE_PATH} /workspace/data.zip

### Create output directory
RUN mkdir -p /output

### Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

### Install unzip package
RUN apt-get install unzip

### Unzip data and move it into the models' input folder (data folder) 
# "-o: overwrite", "-q: quiet", "-d destination"
RUN unzip -o /workspace/data.zip -d /workspace

### Upload/copy license (file must be called gamslice.txt)
## No licence file found or entered!
RUN curl -SL ${LICENSE_PATH} --create-dirs -o /opt/gams/gamslice.txt

### Run model during container start
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/result"]