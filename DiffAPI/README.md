# DiffAPI

Provided are `2` http endpoints (`<host>/v1/diff/<ID>/left` and `<host>/v1/diff/<ID>/right`) that accept
JSON containing base64 encoded binary data on both endpoints.

The provided data is diff-ed and the results are available on a third endpoint
(`<host>/v1/diff/<ID>`). The results provide the following info in JSON format:

- If equal return that
- If not of equal size just return that
- If of same size provide insight in where the diff are, actual diffs are not needed.
	- So mainly offsets + length in the data


## Here are some examples (input / output):

|    | Request                  | Response			|
| -|- |- |
| 1. | `GET /v1/diff/1`       | `404 Not Found`   |
| 2. | `PUT /v1/diff/1/left `   | `201 Created`     |
|    |`{`						|					|
|    |`"data": "AAAAAA=="`		|					|
|    |`}`						|					|
| 3. |`GET /v1/diff/1`			| `404 Not Found`   |
| 4. | `PUT /v1/diff/1/right`   | `201 Created`     |
|    | `{`						|					|
|	 | `"data": "AAAAAA=="`		|					|
|    | `}`						|					|
| 5. | `GET /v1/diff/1`			| `200 OK`			|
|	 |							| `{`				|
|	 |							| `"diffResultType": "Equals"` |
|	 |							| `}`				|
| 6. | `PUT /v1/diff/1/right`   | `201 Created`		|
|	 | `{`						|					|
|	 | `"data": "AQABAQ=="`	    |					|
|	 | `}`						|					|
| 7. | `GET /v1/diff/1`			| `200 OK`			|
|	 |							| `{`				|
|	 |							| `"diffResultType": "ContentDoNotMatch",` |
|	 |							| `"diffs": [`      |
|	 |							| `{`				|
|	 |							| `"offset": 0,`    |
|	 |							| `"length": 1`     |
|	 |							| `},`				|
|	 |							| `{`				|
|	 |							| `"offset": 2,`    |
|	 |							| `"length": 2`		|
|	 |							| `}`				|
|	 |							| `]`				|
|	 |							| `}`				|
| 8. | `PUT /v1/diff/1/left`	| `201 Created`		|
|	 | `{`						|					|
|	 | `"data": "AAA="`			|					|
|	 | `}`						|					|
| 9. | `GET /v1/diff/1`			| `200 OK`			|
|	 |							| `{`				|
|	 |							| `"diffResultType": "SizeDoNotMatch"` |
|	 |							| `}`				|
| 10.| `PUT /v1/diff/1/left`	| `400 Bad Request` |
|	 | `{`						|					|
|	 | `"data": null`			|					|
|	 | `}`						|					|



### Notes

Functionality is under integration tests (not full code coverage) and internal logic is under unit tests (not full code coverage).
