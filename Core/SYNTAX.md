() = Priority

* = 0 or more
+ = 1 or more
? = 0 or 1

## Syntax
 
- **Expression**: ArithmeticExpression
   
- **ArithmeticExpression**: Term ((ADD|SUBTRACT) Term)*

- **Variable**: VARIABLE (IDENTIFIER|KEYWORD|BaseAtom)
    IDENTIFIER

- **Term**: Factor ((MULTIPLY|DIVIDE) Factor)*
  
- **Factor**: (ADD|SUBTRACT)? Factor
    Atom
 
- **Atom**: BaseAtom
 
- **BaseAtom**: Number|Variable
    OPEN_PARENTHESIS Expression CLOSE_PARENTHESIS