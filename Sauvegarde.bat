@echo off
cd /d F:\source\repos\Poker

set /p MSG=Commentaire (Entrée = date) :

if "%MSG%"=="" set MSG=Sauvegarde %date% %time%

git add .

git commit -m "%MSG%"

git log --oneline -1

pause