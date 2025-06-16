# Documentaci�n del Proyecto Pixel 

## Wall-E
El proyecto Pixel Wall-E es una aplicaci�n interactiva que permite a los usuarios crear "pixel art" mediante la escritura de c�digo en un lenguaje de programaci�n dise�ado espec�ficamente para esta tarea. El sistema est� estructurado en m�dulos que trabajan en conjunto para interpretar el c�digo, manipular un lienzo digital y presentar los resultados visualmente.

## Arquitectura y Componentes Clave
La aplicaci�n se adhiere a una arquitectura modular, conformada por los siguientes componentes esenciales:

### Analizador L�xico (Lexer) (Core.Lexer):
#### Prop�sito: 
Es la fase inicial del procesamiento del c�digo fuente. Su funci�n es convertir la secuencia de caracteres de entrada en una serie de unidades l�xicas (tokens) con significado.
#### Caracter�sticas Notables: 
Implementa un sistema robusto para identificar y clasificar elementos del lenguaje como identificadores, valores num�ricos, operadores (incluyendo combinaciones como &&, ||, **, <=, >=, !=, ==), cadenas de texto y palabras clave reservadas. Adem�s, es capaz de detectar y reportar errores en la estructura l�xica del c�digo.

### Analizador Sint�ctico (Parser) (Core.Parser):
#### Prop�sito: 
Recibe la secuencia de tokens del Lexer y construye una representaci�n jer�rquica del c�digo, conocida como �rbol de Sintaxis Abstracta (AST). Este �rbol captura la estructura gramatical del programa.
#### Caracter�sticas Notables: 
Emplea una t�cnica de an�lisis descendente recursivo para interpretar la gram�tica del lenguaje, permitiendo el reconocimiento de estructuras como declaraciones de funciones, asignaciones de variables, llamadas a funciones, definici�n de etiquetas y sentencias de salto condicional. Tambi�n maneja la precedencia de operadores en las expresiones.

### �rbol de Sintaxis Abstracta (AST) (Core.AST):
#### Prop�sito: 
Define la estructura de datos que representa el programa de forma abstracta, una vez ha sido analizado sint�cticamente. Cada nodo del AST corresponde a un elemento de la gram�tica del lenguaje, como una instrucci�n o una expresi�n.
#### Caracter�sticas Notables: 
La implementaci�n del AST es inherentemente extensible, lo que facilita la incorporaci�n de nuevas instrucciones, expresiones o tipos de nodos con un impacto m�nimo en el c�digo existente. Incluye nodos especializados para operaciones de asignaci�n, retorno de valores, saltos de ejecuci�n, y una jerarqu�a para expresiones y funciones. Clases como ProgramAST y Script gestionan el flujo y la organizaci�n del c�digo.

### Utilidades del Sistema y Editor de Im�genes (Core.Utils.SystemClass, Core.Utils.ImageEditor, Core.Utils.Error, Core.Utils.TokenSystem):
#### Prop�sito: 
Este conjunto de componentes provee funcionalidades auxiliares para manejar tipos de datos espec�ficos del lenguaje (por ejemplo, un tipo que puede ser indistintamente un entero o un booleano, o un tipo que representa la ausencia de valor), herramientas para la manipulaci�n del lienzo de dibujo y un sistema integral para la gesti�n y reporte de errores.
#### Caracter�sticas Notables: 
La clase IntegerOrBool ejemplifica una soluci�n flexible para la conversi�n y coexistencia de tipos num�ricos y booleanos. ImageEditor act�a como una capa de abstracci�n sobre una librer�a de procesamiento de im�genes (SixLabors.ImageSharp), simplificando las operaciones de dibujo. El sistema de errores (CompilingError, ExecutionError) ofrece mensajes detallados, incluyendo la ubicaci�n exacta del error en el c�digo fuente, lo que facilita la depuraci�n.

### Analizador Sem�ntico (Core.Semantic):
#### Prop�sito: 
Se encarga de verificar la coherencia y validez l�gica del programa, analizando el AST para detectar inconsistencias en el uso de tipos, el alcance de las variables y la correcci�n de las llamadas a funciones.
#### Caracter�sticas Notables: 
Este componente realiza validaciones cr�ticas, como asegurar que toda ejecuci�n comience con una instrucci�n inicial espec�fica (Spawn), verificar la correspondencia de argumentos en las llamadas a funciones y garantizar la compatibilidad de tipos en asignaciones y expresiones. Su dise�o permite la detecci�n de m�ltiples errores sem�nticos en una sola pasada.

### Ejecutor (Core.Executor):
Prop�sito: Es el motor que interpreta y ejecuta las instrucciones contenidas en el AST, interactuando directamente con el subsistema de edici�n de im�genes para plasmar los resultados en el lienzo.
Caracter�sticas Notables: Procesa las operaciones definidas en el lenguaje, gestionando el estado del lienzo digital. Incorpora un mecanismo para registrar funciones de sistema, que son las operaciones nativas del editor de im�genes accesibles desde el lenguaje.

### Motor de Gr�ficos (Core.PixelWallE):
#### Prop�sito: 
Sirve como el puente principal entre el lenguaje de programaci�n y las operaciones gr�ficas en el lienzo. Administra la posici�n virtual del "robot" de dibujo (Wall-E) y las propiedades del pincel (color, tama�o, tipo).
#### Caracter�sticas Notables: 
Implementa las funciones de dibujo de bajo nivel, como mover el "robot" a una posici�n espec�fica, dibujar en el lienzo, y aplicar distintos colores y estilos de pincel.

### Controlador (Core.Controller):
#### Prop�sito: 
Coordina el flujo general de la aplicaci�n, desde la carga y el an�lisis del c�digo hasta su ejecuci�n y la gesti�n de las actualizaciones en la interfaz de usuario.
#### Caracter�sticas Notables: 
Integra y orquesta el funcionamiento del Lexer, Parser, Analizador Sem�ntico y Ejecutor. Se encarga de la compilaci�n y ejecuci�n de programas, as� como de la administraci�n de m�ltiples archivos de script y la comunicaci�n con la capa de presentaci�n para actualizar el estado visual del lienzo.

### Interfaz de Usuario (WebApp):
#### Prop�sito: 
Proporciona la experiencia visual al usuario, incluyendo un editor de texto para escribir c�digo, un lienzo de dibujo para visualizar los resultados, una consola para mostrar errores y mensajes, y controles para la interacci�n con la aplicaci�n.
#### Caracter�sticas Notables: 
Desarrollada con Blazor, ofrece un editor de texto interactivo con funcionalidades como la sincronizaci�n de l�neas y la visualizaci�n en tiempo real de los resultados en el lienzo. La integraci�n con JavaScript (ideInterop.js) facilita operaciones como la carga y descarga de archivos, y la gesti�n de men�s contextuales, contribuyendo a una experiencia de usuario fluida y receptiva.

### Dise�o y Flexibilidad del Proyecto
El proyecto Pixel Wall-E ha sido dise�ado con un fuerte enfoque en la modularidad y la aplicaci�n de buenas pr�cticas de ingenier�a de software. Esto se traduce en una "extensibilidad de c�digo" notable, permitiendo que nuevas instrucciones, funciones o tipos de expresiones puedan ser incorporados al lenguaje con relativa facilidad y sin afectar la integridad de los componentes existentes. La gesti�n de errores es una prioridad, ofreciendo diagn�sticos precisos que incluyen la ubicaci�n del error en el c�digo fuente y la capacidad de reportar m�ltiples problemas, lo que enriquece la experiencia de depuraci�n para los desarrolladores. En caso de errores durante la ejecuci�n, el lienzo conserva el estado de todas las operaciones completadas hasta ese momento. La aplicaci�n tambi�n soporta la importaci�n y exportaci�n de archivos de c�digo, lo que la hace una herramienta completa para la creaci�n y experimentaci�n con "pixel art" program�tico.