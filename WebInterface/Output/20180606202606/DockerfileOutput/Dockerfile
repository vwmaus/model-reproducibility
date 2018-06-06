FROM iiasa/gams:latest
MAINTAINER "Victor Maus" maus@iiasa.ac.at

# Gams license path
ENV LICENSE_PATH=#LICENSE_PATH#

# Get github repository from web inteface
ENV GITHUB_USER=ptrkrnstnr

# Get github repository from web interface 
ENV GITHUB_REPO=corefx

# Get model version from web interface 
ENV GITHUB_RELEASE=PortablePdbsAndSourceServer

# Get data version from web interface 
ENV GEONODE_DATA=#DATA_VERSION#

# Clone github repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

# Set workspace 
WORKDIR /workspace 

# Create output directory
RUN mkdir -p /output

# Download data from geonode
RUN curl -SL http://geonode_geonode_1/documents/${GEONODE_DATA}/download --create-dirs -o /workspace/data.zip
#RUN curl -SL "http://localhost:8011/documents/${GEONODE_DATA}/download" --create-dirs -o /workspace/data.zip
# or 
# COPY userinputfile (data.zip) /workspace/data.zip

# Unzip data and move it into the models' input folder (data folder) / -q = quiet
RUN unzip -o /workspace/data.zip -d /workspace/data

# remove line 28 to 35 if the user does not input any data
# if the model does not need data. 

# so 3 options:
# 1. data comes from URL
# 2. data comes from user upload
# 3. data not needed (model does not need input)

# Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

# Upload/copy license (file must be called gamslice.txt)
COPY ${LICENSE_PATH} /opt/gams/gams/gamslice.txt

# Run model during container start 
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/output"]