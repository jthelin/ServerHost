# docker build . --file Dockerfile --tag serverhost

FROM mono:5

RUN mono --version

WORKDIR /home/app/src

COPY . .

RUN ./Build.sh

RUN ./Test.sh

CMD [ "bash" ]
