# Documentación del Proyecto Pixel 

## Wall-E
El proyecto Pixel Wall-E es una aplicación interactiva que permite a los usuarios crear "pixel art" mediante la escritura de código en un lenguaje de programación diseñado específicamente para esta tarea. El sistema está estructurado en módulos que trabajan en conjunto para interpretar el código, manipular un lienzo digital y presentar los resultados visualmente.

## Arquitectura y Componentes Clave
La aplicación se adhiere a una arquitectura modular, conformada por los siguientes componentes esenciales:

### Analizador Léxico (Lexer) (Core.Lexer):
#### Propósito: 
Es la fase inicial del procesamiento del código fuente. Su función es convertir la secuencia de caracteres de entrada en una serie de unidades léxicas (tokens) con significado.
#### Características Notables: 
Implementa un sistema robusto para identificar y clasificar elementos del lenguaje como identificadores, valores numéricos, operadores (incluyendo combinaciones como &&, ||, **, <=, >=, !=, ==), cadenas de texto y palabras clave reservadas. Además, es capaz de detectar y reportar errores en la estructura léxica del código.

### Analizador Sintáctico (Parser) (Core.Parser):
#### Propósito: 
Recibe la secuencia de tokens del Lexer y construye una representación jerárquica del código, conocida como Árbol de Sintaxis Abstracta (AST). Este árbol captura la estructura gramatical del programa.
#### Características Notables: 
Emplea una técnica de análisis descendente recursivo para interpretar la gramática del lenguaje, permitiendo el reconocimiento de estructuras como declaraciones de funciones, asignaciones de variables, llamadas a funciones, definición de etiquetas y sentencias de salto condicional. También maneja la precedencia de operadores en las expresiones.

### Árbol de Sintaxis Abstracta (AST) (Core.AST):
#### Propósito: 
Define la estructura de datos que representa el programa de forma abstracta, una vez ha sido analizado sintácticamente. Cada nodo del AST corresponde a un elemento de la gramática del lenguaje, como una instrucción o una expresión.
#### Características Notables: 
La implementación del AST es inherentemente extensible, lo que facilita la incorporación de nuevas instrucciones, expresiones o tipos de nodos con un impacto mínimo en el código existente. Incluye nodos especializados para operaciones de asignación, retorno de valores, saltos de ejecución, y una jerarquía para expresiones y funciones. Clases como ProgramAST y Script gestionan el flujo y la organización del código.

### Utilidades del Sistema y Editor de Imágenes (Core.Utils.SystemClass, Core.Utils.ImageEditor, Core.Utils.Error, Core.Utils.TokenSystem):
#### Propósito: 
Este conjunto de componentes provee funcionalidades auxiliares para manejar tipos de datos específicos del lenguaje (por ejemplo, un tipo que puede ser indistintamente un entero o un booleano, o un tipo que representa la ausencia de valor), herramientas para la manipulación del lienzo de dibujo y un sistema integral para la gestión y reporte de errores.
#### Características Notables: 
La clase IntegerOrBool ejemplifica una solución flexible para la conversión y coexistencia de tipos numéricos y booleanos. ImageEditor actúa como una capa de abstracción sobre una librería de procesamiento de imágenes (SixLabors.ImageSharp), simplificando las operaciones de dibujo. El sistema de errores (CompilingError, ExecutionError) ofrece mensajes detallados, incluyendo la ubicación exacta del error en el código fuente, lo que facilita la depuración.

### Analizador Semántico (Core.Semantic):
#### Propósito: 
Se encarga de verificar la coherencia y validez lógica del programa, analizando el AST para detectar inconsistencias en el uso de tipos, el alcance de las variables y la corrección de las llamadas a funciones.
#### Características Notables: 
Este componente realiza validaciones críticas, como asegurar que toda ejecución comience con una instrucción inicial específica (Spawn), verificar la correspondencia de argumentos en las llamadas a funciones y garantizar la compatibilidad de tipos en asignaciones y expresiones. Su diseño permite la detección de múltiples errores semánticos en una sola pasada.

### Ejecutor (Core.Executor):
Propósito: Es el motor que interpreta y ejecuta las instrucciones contenidas en el AST, interactuando directamente con el subsistema de edición de imágenes para plasmar los resultados en el lienzo.
Características Notables: Procesa las operaciones definidas en el lenguaje, gestionando el estado del lienzo digital. Incorpora un mecanismo para registrar funciones de sistema, que son las operaciones nativas del editor de imágenes accesibles desde el lenguaje.

### Motor de Gráficos (Core.PixelWallE):
#### Propósito: 
Sirve como el puente principal entre el lenguaje de programación y las operaciones gráficas en el lienzo. Administra la posición virtual del "robot" de dibujo (Wall-E) y las propiedades del pincel (color, tamaño, tipo).
#### Características Notables: 
Implementa las funciones de dibujo de bajo nivel, como mover el "robot" a una posición específica, dibujar en el lienzo, y aplicar distintos colores y estilos de pincel.

### Controlador (Core.Controller):
#### Propósito: 
Coordina el flujo general de la aplicación, desde la carga y el análisis del código hasta su ejecución y la gestión de las actualizaciones en la interfaz de usuario.
#### Características Notables: 
Integra y orquesta el funcionamiento del Lexer, Parser, Analizador Semántico y Ejecutor. Se encarga de la compilación y ejecución de programas, así como de la administración de múltiples archivos de script y la comunicación con la capa de presentación para actualizar el estado visual del lienzo.

### Interfaz de Usuario (WebApp):
#### Propósito: 
Proporciona la experiencia visual al usuario, incluyendo un editor de texto para escribir código, un lienzo de dibujo para visualizar los resultados, una consola para mostrar errores y mensajes, y controles para la interacción con la aplicación.
#### Características Notables: 
Desarrollada con Blazor, ofrece un editor de texto interactivo con funcionalidades como la sincronización de líneas y la visualización en tiempo real de los resultados en el lienzo. La integración con JavaScript (ideInterop.js) facilita operaciones como la carga y descarga de archivos, y la gestión de menús contextuales, contribuyendo a una experiencia de usuario fluida y receptiva.

### Diseño y Flexibilidad del Proyecto
El proyecto Pixel Wall-E ha sido diseñado con un fuerte enfoque en la modularidad y la aplicación de buenas prácticas de ingeniería de software. Esto se traduce en una "extensibilidad de código" notable, permitiendo que nuevas instrucciones, funciones o tipos de expresiones puedan ser incorporados al lenguaje con relativa facilidad y sin afectar la integridad de los componentes existentes. La gestión de errores es una prioridad, ofreciendo diagnósticos precisos que incluyen la ubicación del error en el código fuente y la capacidad de reportar múltiples problemas, lo que enriquece la experiencia de depuración para los desarrolladores. En caso de errores durante la ejecución, el lienzo conserva el estado de todas las operaciones completadas hasta ese momento. La aplicación también soporta la importación y exportación de archivos de código, lo que la hace una herramienta completa para la creación y experimentación con "pixel art" programático.