sample link - https://localhost:5001/sanctions?name=vladimir%20naumov&name=bob&sanctionsList=eu

publish to test with
dotnet publish -c Release --output \\192.168.1.8\d$\domains\api.sanctions

publish to live with....
dotnet publish -c Release --output \\lcapp2016\api$\api.sanctions

Some sample queries
https://localhost:5001/sanctions?name=ZABIN,%20Sultan&name=bob&sanctionsList=usa
https://localhost:5001/sanctions?name=Broadway Market (1936) Limited&name=bob&sanctionsList=usa
https://localhost:5001/sanctions?name=AEROCARIBBEAN%20AIRLINES&name=bob&sanctionsList=usa
https://localhost:5001/sanctions?name=Organizaci%C3%B3n%20Revolucionaria&name=bob&sanctionsList=eu

TODO:
suggest returning a 400 if the source file is out of date by a number of days

test cors with
```
curl -v https://localhost:5001/sanctions -I -X "OPTIONS" -H 'Origin: http://localhost' -H 'Access-Control-Request-Method: GET'
```
headers should output
```
< Access-Control-Allow-Credentials: true
< Access-Control-Allow-Origin: http://localhost
```
and you will see relevant entries in the console log

If running curl in terminal in windows, you may need to do
```
Remove-item alias:curl
```
as terminal aliases curl to powershell's Invoke-WebRequest

test cors in live with 
```
curl --ntlm -u lcdomain\sangell -v https://www.conveyancinginsurance.co.uk/api.sanctions/sanctions -I  -X "OPTIONS" -H 'Origin: https://www.conveyancinginsurance.co.uk' -H 'Access-Control-Request-Method: GET'
```
or in a browser in dev tools
```
fetch('https://www.conveyancinginsurance.co.uk/api.sanctions/sanctions')
```

do a js fetch and pass ntlm credentia
```
fetch('https://www.conveyancinginsurance.co.uk/api.sanctions/sanctions', {credentials: 'include'})
```
