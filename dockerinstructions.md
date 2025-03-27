# Docker Instructions

Run the docker container with:
sudo docker run -it --rm --network host -v /home/fluffy/Documents/Projects/Bellrock/NetTechnicalTask:/app -w /app mcr.microsoft.com/dotnet/sdk:7.0

then
dotnet run --project DotNetInterview.API
