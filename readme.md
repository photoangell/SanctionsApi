sample link - https://localhost:5001/sanctions?name=vladimir%20naumov&name=bob&sanctionsList=eu

publish to test with
dotnet publish -c Release --output \\192.168.1.8\d$\domains\api.sanctions

publish to live with....
dotnet publish -c Release --output \\lcapp2016\api$\api.sanctions

Some sample queries
https://localhost:5001/sanctions?name=ZABIN,%20Sultan&name=bob&sanctionsList=usa
https://localhost:5001/sanctions?name=Broadway Market (1936) Limited&name=bob&sanctionsList=usa
https://localhost:5001/sanctions?name=AEROCARIBBEAN%20AIRLINES&name=bob&sanctionsList=usa

TODO:
suggest returning a 400 if the source file is out of date by a number of days