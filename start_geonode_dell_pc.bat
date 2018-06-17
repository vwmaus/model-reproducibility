@echo off

CD "C:\GithubRepos\geonode"

echo docker-compose start
docker-compose start

echo docker-compose exec django django-admin.py migrate --noinput
docker-compose exec django django-admin.py migrate --noinput

set /p DUMMY=Hit ENTER to continue...