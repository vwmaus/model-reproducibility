FROM iiasa/gams:latest
MAINTAINER "Victor Maus" maus@iiasa.ac.at

# Gams license path
ENV LICENSE_PATH=#LICENSE_PATH#

# Get github repository from web inteface
ENV GITHUB_USER=ptrkrnstnr

# Get github repository from web interface 
ENV GITHUB_REPO=transport-model

# Get model version from web interface 
ENV GITHUB_RELEASE=v1.0

# Get data version from web interface 
ENV GEONODE_DATA=3

# Clone github repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

# Set workspace 
WORKDIR /workspace 

# Create output directory
RUN mkdir -p /output

# Download data from geonode
RUN curl -SL "http://geonode_geonode_1/documents/${GEONODE_DATA}/download" --create-dirs -o /data/data.zip
#RUN curl -SL http://localhost:8011/documents/${GEONODE_DATA}/download --create-dirs -o /data/data.zip

# Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

# Upload/copy license (file must be called gamslice.txt)
#COPY ${LICENSE_PATH} /opt/gams/gams/gamslice.txt

# Run model during container start 
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/output"]

