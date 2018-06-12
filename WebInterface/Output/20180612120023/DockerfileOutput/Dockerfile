FROM iiasa/gams:latest
MAINTAINER "Victor Maus" maus@iiasa.ac.at

# Gams license path
ENV LICENSE_PATH=

# Get github repository from web inteface
ENV GITHUB_USER=ptrkrnstnr

# Get github repository from web interface 
ENV GITHUB_REPO=corefx

# Get model version from web interface 
ENV GITHUB_RELEASE=PortablePdbsAndSourceServer

# Input Data File Path or link of external input data
ENV INPUT_DATA_FILE_PATH=

# Copy model input data to workspace directory
## No model input data file uploaded
COPY data:/app/wwwroot/uploads /workspace

# Set workspace 
WORKDIR /workspace

# Clone github repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

# Create output directory
RUN mkdir -p /output

# Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

# Download model input data to workspace directory
# No URL for model input data file provided

# Unzip data and move it into the models' input folder (data folder) / -q = quiet
# No input data file provided

# Upload/copy license (file must be called gamslice.txt)
# No licence file found or entered!

# Run model during container start 
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/result"]