### Get base image
FROM iiasa/gams:#GAMS_VERSION#

### Set maintainer
MAINTAINER "Victor Maus" maus@iiasa.ac.at

### Program license path
ENV LICENSE_PATH=#LICENSE_PATH#

### Get GitHub repository from web interface
ENV GITHUB_USER=#GITHUB_USER#

### Get GitHub repository from web interface 
ENV GITHUB_REPO=#MODEL#

### Get model version from web interface 
ENV GITHUB_RELEASE=#MODEL_VERSION#

### Input data file path or link of external input data
ENV INPUT_DATA_FILE_PATH=#INPUT_DATA_FILE_PATH#

### Set workspace 
WORKDIR /workspace

### Set root user
USER root

### Clone GitHub repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

### Copy/download uploaded model input data to workspace directory
RUN curl -SL ${INPUT_DATA_FILE_PATH} --create-dirs -o /workspace/data.zip

### Create output directory
RUN mkdir -p /output

### Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

### Remove old data folder
RUN rm -rf /workspace/data

### Unzip data and move it into the models' input folder (data folder) 
# "-o: overwrite", "-q: quiet", "-d destination"
RUN unzip -o /workspace/data.zip -d /workspace/data

### Upload/copy license (file must be called gamslice.txt)
RUN curl -SL ${LICENSE_PATH} --create-dirs -o /opt/gams/gamslice.txt

### Reconfigure GAMS if user provided license
CMD ["/bin/bash", "-c", "gamsinst -a"]

### Run model during container start
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/result"]