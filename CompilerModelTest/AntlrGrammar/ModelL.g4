grammar ModelL;


programm:	LBRACE	((declaration | statement) | ';')* RBRACE	 
	;

//-------ОПИСАНИЕ---------------
declaration
	:	dim identificator (',' identificator)* type=(INT|REAL|BOOL) ';'
	;
dim 
	:	'dim'	
	;
identificator
	:	ID	
	;
//type
//	:	int|real|bool
//	;	
BOOL	
	:	'boolean'
	;
REAL	
	:	'real'	
	;
INT	
	:	'integer'
	;
//-----------------------------

//----------Операторы----------

statement
	:	simple_statement | internal_statemenet		
	;
simple_statement
	:	assigment | for  | if | while | read  | write 	
	;

internal_statemenet
	:	LBRACE (statement )* RBRACE
	;

//fragment INTERNAL_STATEMENT
//	:	('\r\n'|':') statement 	
//	;

assigment
	:	identificator as expression	';'
	;
as	:	AS	
	;	
if	:	IF expression statement (ELSE statement)?
	;
for	:	FOR assigment TO expression DO statement
	;
while	:	WHILE expression DO statement
	;
read	:	READ '(' identificator (',' identificator)* ')' ';' 	
	;
write	:	WRITE '(' expression (',' expression)* ')' ';'
	;
//-----------------------------

//---------Выражение-----------
expression
	:	  primary 
		| unary primary
		| left=expression op=('*'|'/'|'and')		right=expression
		| left=expression op=('+'|'-'|'or')			right=expression
		| left=expression op=('<>'|'=')				right=expression 
		| left=expression op=('<'|'>'|'<='|'>=')	right=expression
	;

primary	:	 
		  inner_expression
		| identificator
		| number 
		| boolean 
		;

inner_expression
	: '(' expression ')'
	;
/*
add:	ADD;
sub:	SUB;
mul:	MUL;
div:	DIV;
*/
/*
operand	:	summary (add summary)?	#AddSubOr
	;
summary	:	multiplier (mul multiplier)? #MulDivAnd
	;
multiplier
	:	identificator | number | boolean | unary multiplier | '(' expression ')'
	;

add	:	ADDITIONS
	;
mul	:	MULTIPLICATION
	;
relations
	:	RELATIONS	
	;	
MULTIPLICATION
	:	'*'|'/'|'and'	
	;
ADDITIONS
	:	'+'|'-'|'or'	
	;
RELATIONS
	:	'<'|'>'|'<='|'>='|'<>'|'='	
	;
*/
//-----------------------------


//----------------------------------------------------------------------------
// KEYWORDS
//----------------------------------------------------------------------------

	
	FALSE : 'false';
	TRUE : 'true';
	IF : 'if';
	THEN : 'then';
	ELSE : 'else';
	WHILE : 'while';
	DO:	'do';
	FOR: 'for';
	TO:	'to';
	NOT : 'not';
	WRITE: 'write';
	READ:	'read';
	AS: 'as';
	ADD: '+';
	SUB: '-';
	AND: 'and';
	MUL: '*';
	DIV: '/';
	OR:	'or';
	EQUAL: '=';
	NEQ: '<>';
	MR: '>';
	LS: '<';
	MRE: '>=';
	LE: '<=';

boolean	:	TRUE|FALSE
	;
unary	:	NOT	
	;
//BOOLEAN	:	'true'|'false'	
//	;
//NOT	:	'not'	
//	;

ID	:	('a'..'z'|'A'..'Z') ('a'..'z'|'A'..'Z'|'0'..'9')*	
	;
	
number	:	double |integer
	;
integer	:	INTEGER
	;
double	:	DOUBLE	
	;

DOUBLE	:	DIGIT+ EXPONENT | (DIGIT)* ('.') DIGIT+ EXPONENT? 
	;
EXPONENT:	('E'|'e') ('+'|'-')? DIGIT+	
	;

INTEGER	:	DECNUMBER | OCTNUMBER | HEXNUMBER | BINNUMBER	
	;	
fragment OCTNUMBER       
	:	OCTDIGIT+ OCTLETTER
	;
fragment DECNUMBER	
	:	DIGIT+ DECLETTER?
	;
fragment HEXNUMBER
	:	DIGIT (DIGIT|('A'..'F'|'a'..'f'))* HEXLETTER	
	;	
fragment BINNUMBER
	:	BINDIGIT+ BINLETTER	
	;
fragment DECLETTER
	:	'D'|'d'	
	;
fragment OCTLETTER
	:	'O'|'o'	
	;
fragment HEXLETTER
	:	'H'|'h'		
	;
fragment BINLETTER
	:	'B'|'b'
	;
fragment DIGIT	
	:	('0'..'9')
	;
fragment OCTDIGIT
	:	('0'..'7')	
	;
fragment BINDIGIT
	:	('0'..'1')
	;

LBRACE          : '{';
RBRACE          : '}';

COMMENT	:	'/*' (.)*? '*/'  ->skip
	;
WS:		NEW_LINE RETURN ->skip;	
NEW_LINE:	'\n' ->skip
	;
RETURN	:	'\r' ->skip
	;
TAB	:	'\t' ->skip
	;
SPACE	:	' '  ->skip
	;