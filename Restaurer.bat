@echo off
cd /d F:\source\repos\Poker

git switch master

git reset --hard V1.0

git clean -fd

git status

pause