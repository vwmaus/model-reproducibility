FROM iiasa/gams:latest
MAINTAINER "Victor Maus" maus@iiasa.ac.at

####### Gams license path
ENV LICENSE_PATH=

####### Get github repository from web interface
ENV GITHUB_USER=ptrkrnstnr

####### Get github repository from web interface 
ENV GITHUB_REPO=transport-model

####### Get model version from web interface 
ENV GITHUB_RELEASE=v3.0

####### Input Data File Path or link of external input data
ENV INPUT_DATA_FILE_PATH=/app/wwwroot/uploads/model_input_data/modelInputData.zip

####### Set workspace 
WORKDIR /workspace

####### Clone github repository  
RUN git clone https://github.com/${GITHUB_USER}/${GITHUB_REPO}.git /workspace 

####### Copy model input data to workspace directory
RUN curl -SL http://webinterface/uploads/model_input_data/modelInputData.zip --create-dirs -o /workspace/data.zip

#COPY ${INPUT_DATA_FILE_PATH} /workspace/data.zip
#COPY /uploads/model_input_data/modelInputData.zip /workspace/data.zip
#RUN curl -SL http://webinterface/uploads/model_input_data/modelInputData.zip --create-dirs -o /workspace/data.zip
#RUN curl -SL localhost:5000/uploads/model_input_data/modelInputData.zip /workspace/data.zip

####### Download model input data to workspace directory
## No URL for model input data file provided

####### Create output directory
RUN mkdir -p /output

####### Checkout model version 
RUN git checkout ${GITHUB_RELEASE}

####### Unzip data and move it into the models' input folder (data folder) / -q = quiet
RUN apt-get install unzip

RUN unzip /workspace/data.zip -d /workspace
#RUN unzip -o /workspace/data.zip -d /workspace/data


####### Upload/copy license (file must be called gamslice.txt)
# No licence file found or entered!

####### Run model during container start 
ENTRYPOINT ["/bin/bash", "-c", "gams model.gms gdx=/output/result"]