sample link - https://localhost:5001/sanctions?name=vladimir%20naumov&name=bob&sanctionsList=eu

publish to test with
dotnet publish -c Release --output \\192.168.1.8\d$\domains\api.sanctions

publish to live with....
dotnet publish -c Release --output \\lcapp2016\api$\api.sanctions

TODO: 
suggest returning a 400 if the source file is out of date by a number of days