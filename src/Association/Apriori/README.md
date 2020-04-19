## Apriori Algorithm

### Usage

Example Dataset

```
fieldNames     =>   Bread,Milk,Chips,Mustard,Beer,Diaper,Eggs,Coke
transaction 1# =>   1,1,1,1,0,0,0,0
transaction 2# =>   1,0,1,1,1,1,1,0
transaction 3# =>   0,1,0,0,1,1,0,1
transaction 4# =>   1,1,0,0,1,1,0,0
transaction 5# =>   1,1,1,0,0,1,0,1
transaction 6# =>   1,1,0,1,1,1,0,0
transaction 7# =>   1,1,0,0,0,1,0,1
```

Create DataFields object that contains your data.
```
List<string> Names; // FieldNames
List<bool[]> Rows; // your data
DataFields data = new DataFields(Names, Rows);
Apriori myApriori = new Apriori(data);
```
or just with Data
```
List<bool[]> Rows; // your data
DataFields data = new DataFields(Rows);
```
or 
```
List<int> transaction1 = new List<int>(){ 0, 1, 2, 3 };
List<int> transaction2 = new List<int>(){ 0, 2, 3, 4, 5, 6 };
    .
    .
List<int> transaction7 = new List<int>(){ 0, 1, 5, 7 };    

List<List<int>> Transactions = new List<List<int>>()
				{ transaction1, transaction2, .., transaction7 };

DataFields data = new DataFields(maxColumn, Transactions);
```
And calculations
```
Apriori myApriori = new Apriori(data);
float minimumSupport = 0.4f; // 40% Minimum Support

myApriori.CalculateCNodes(minimumSupport);

myApriori.EachLevelOfNodes // Tables
myApriori.Rules // All rules
```
