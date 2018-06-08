FROM iiasa/gams:latest
MAINTAINER "Victor Maus" maus@iiasa.ac.at

# Gams license path
ENV LICENSE_PATH=

# Get github repository from web inteface
ENV GITHUB_USER=ptrkrnstnr

# Get github repository from web interface 
ENV GITHUB_REPO=transport-model

# Get model version from web interface 
ENV GITHUB_RELEASE=v3.0

# Set workspace 
WORKDIR /app

# Input Data File Path or link of external input data
ENV INPUT_DATA_FILE_PATH=/app/wwwroot/uploads/model_input_data/modelInputData.zip

# Copy model input data to workspace directory
COPY ${INPUT_DATA_FILE_PATH} /workspace/data.zip

# Download model input data to workspace directory
# No URL for model input data file provided

# Upload/copy license (file must be called gamslice.txt)
# No licence file found or entered!

# Unzip data and move it into the models' input folder (data folder) / -q = quiet
RUN unzip -o /workspace/data.zip -d /workspace/data

# Clone github repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

# Set workspace 
WORKDIR /workspace

# Create output directory
RUN mkdir -p /output

# Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

# Run model during container start 
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/result"]