# PruebaVirtualZone

Escape room en primera persona ambientado en el faro de Finisterre, desarrollado en **Unity 6**. El jugador explora la sala, inspecciona notas y objetos, resuelve un candado numérico, mecánicas de peso y un puzzle de haces de luz con espejos.

## Cómo ejecutar

Las escenas ya están configuradas en **File → Build Settings** en este orden:

| # | Escena | Estado |
|---|---|---|
| 0 | `Menu.unity` | Habilitada — menú principal |
| 1 | `EscapeRoomNivel.unity` | Habilitada — nivel principal |
| 2 | `Completado.unity` | Habilitada — pantalla de final |
| — | `EscapeRoom.unity` | Deshabilitada — prototipo anterior (sala única) |
| — | `SampleScene.unity` | Escena vacía de plantilla, fuera del build |

---

## Controles

| Acción | Teclas |
|---|---|
| Moverse | `W` `A` `S` `D` |
| Mirar | Ratón |
| Interactuar (coger/soltar, puertas, candado, espejos, inspeccionar) | `E` |
| Cancelar / salir de la inspección | `Esc` o clic derecho |
| Agacharse | `Ctrl izquierdo` o `C` |
| Saltar la intro | Cualquier tecla o clic |

La interacción funciona por raycast a 2,6 m: un texto de aviso en pantalla indica la acción disponible.

---

## Argumento

Nuestro personaje se adentra en el faro de Finisterre para buscar la clave de la inmortalidad.

---

## Sistemas de juego

- **Exploración e inspección** (`Inspectable`, `PlayerInteractor`) — con `E` se enfoca un objeto frente a la cámara y se rota con el ratón para leer notas o examinar detalles. `Esc` vuelve al control normal.
- **Objetos agarrables** (`Grabbable`) — coger y soltar con `E`; los barriles y demás objetos sirven de peso.
- **Caja fuerte** (`SafeDoor` + `CombinationLock` + `KeypadButton`) — candado numérico con dígitos `0-9`, Clear y Confirm; al acertar la tapa gira sobre su bisagra y descubre la llave.
- **Llave** (`Key`) — se recoge con `E` y, apuntando a una puerta que la requiere, se usa con `E`.
- **Placas de peso** (`PressurePlate`) — se activan cuando el encima supera el umbral (el jugador también cuenta) y se desactivan si lo quitas. Mueven `SlidingPanel` (panel que oculta un hueco) y puertas automáticas.
- **Puertas** (`ExitDoor`, contrato `IUnlockable`) — tres modos: con llave, con placa o desbloqueadas por puzzles; la luz de al lado indica su estado (naranja trabada / verde desbloqueada). En la escena del nivel la puerta del faro se abre al resolver el puzzle de luz.
- **Puzzle de haz** (`LightEmitter`, `Mirror`, `LightReceiver`, `ConditionGate`) — el emisor lanza un haz que rebota en los espejos (giran 45° con `E`); paredes, el jugador u objetos lo cortan y se recalcula cada frame. Los receptores pasan de rojo a verde al recibir el haz y desbloquean sus objetivos; la compuerta AND solo actúa cuando hay varias fuentes activas a la vez.
- **Final** (`LevelFinish` + `PantallaCompletado`) — trigger tras la salida del faro; carga la pantalla final y libera el ratón para los botones.

---

## Flujo

```
Menu ──Jugar──▶ EscapeRoomNivel                Completado
                  │                              ▲   │
                  ├─ Intro (Timeline, saltable)  │   ├─ Repetir ──▶ EscapeRoomNivel
                  ├─ Explorar / notas / puzzle   │   └─ Volver al menú ──▶ Menu
                  └─ Cruzar la salida del faro ──┘
```

---

## Estructura del proyecto

```
Assets/
├── Art/
│   ├── Materials/        Materiales de la escena
│   ├── Modelos/          Barril.fbx · Silla.fbx  (modelos propios)
│   └── Textures/         Glow y texturas varias
├── Audio/
│   ├── Ambiente/         RainSound.wav
│   ├── Espejo/           LightOnSound.wav · RotateSound.wav
│   ├── Musica/           menu_theme.wav
│   ├── Padlock/          BipSound.wav · CompleteSound.wav · ErrorSound.wav
│   ├── PlacaPresion/     HeavyObjectFall.wav
│   └── Puerta/           DoorUnlock.wav
├── Prefabs/              BarrilA · CajaFuerte · EmisorA · Espejos · NotaMesa
│                         PlacaPeso · Puertas · Receptores
├── Scenes/               Menu · EscapeRoomNivel · Completado · EscapeRoom
│                         IntroTimeline.playable
├── Scripts/              Lógica del juego (ver abajo) + Player/
├── Settings/             Perfiles y assets de URP (PC / Mobile)
└── TextMesh Pro/         Fuentes y shaders de TextMeshPro
```

---

## Arquitectura de código

Todos los scripts viven en `Assets/Scripts/` (namespace `LighthouseEscape`).

**Jugador**

| Script | Función |
|---|---|
| `Player/PlayerController` | Movimiento WASD y cámara en primera persona (Input System); se congela durante las inspecciones y bloquea el cursor en el nivel |
| `Player/PlayerInteractor` | Interacción con `E`: agarrar, puertas, candado, espejos, inspeccionar; dibuja los avisos en pantalla |

**Interacción**

| Script | Función |
|---|---|
| `Grabbable` | Marca de objeto agarrable (coger/soltar con `E`) |
| `Inspectable` | Enfoca el objeto frente a la cámara y lo rota con el ratón |
| `IUnlockable` | Contrato `Unlock()`/`Lock()`; lo implementan puertas, tapas y candados |

**Puzzles**

| Script | Función |
|---|---|
| `CombinationLock` | Candado numérico: dígito a dígito, al completar comprueba el código (acierto → sonido + desbloqueo; fallo → borra el intento) |
| `KeypadButton` | Tecla del candado: 0-9, Clear y Confirm |
| `SafeDoor` | Tapa de la caja fuerte; gira sobre su bisagra al acertar y descubre la llave |
| `Key` | Llave de la puerta que la requiere |
| `PressurePlate` | Placa de presión por umbral de peso (OverlapBox; funciona con CharacterController) |
| `SlidingPanel` | Panel que sube con peso de la placa y baja si lo quitan |
| `LightEmitter` | Emisor del haz; rebota en espejos, lo cortan paredes/jugador/objetos |
| `Mirror` | Espejo del haz, gira 45° con `E` |
| `LightReceiver` | Receptor rojo/verde; al recibir el haz reproduce su sonido y desbloquea sus objetivos; si se corta, vuelve a trabarlos |
| `ConditionGate` | Compuerta AND: solo desbloquea con `requiredSources` fuentes activas a la vez |

**Flujo y escena**

| Script | Función |
|---|---|
| `ExitDoor` | Puerta con llave / placa / puzzle; luz de estado naranja-verde; gira sobre su bisagra con `E` o sola |
| `LevelFinish` | Trigger de final: al atravesarlo carga la pantalla de completado |
| `PantallaCompletado` | Pantalla final: Repetir / Volver al menú; libera el cursor |
| `MenuPrincipal` | Menú: Jugar carga el nivel, Salir cierra la aplicación |
| `IntroController` | Intro con Timeline: inmoviliza al jugador, saltable con cualquier tecla |
| `GameFeedback` | Reproductor central de sonidos: reproduce los clips asignados en el inspector (nunca genera audio en runtime) |

---

## Arte y sonido

- **Modelos propios:** `Art/Modelos/Barril.fbx` (instanciado como prefab `BarrilA`) y `Art/Modelos/Silla.fbx`. Son los dos únicos modelos 3D del repositorio; el resto de la escena se compone con primitivas de Unity y formas propias.
- **Sonidos** (WAV, asignados en el inspector de cada componente):

| Clip | Uso |
|---|---|
| `RainSound` | Lluvia de ambiente (menú y nivel) |
| `menu_theme` | Música de fondo en bucle del nivel (GameObject `AudioMusica`) |
| `BipSound` | Tecla del candado |
| `CompleteSound` | Código correcto y final de nivel |
| `ErrorSound` | Código incorrecto |
| `DoorUnlock` | Desbloqueo de puerta |
| `HeavyObjectFall` | Peso sobre la placa |
| `RotateSound` | Giro de espejo |
| `LightOnSound` | Receptor energizado |

---

## Trabajo con IA

La IA es parte de mi flujo de trabajo, pero siempre por detrás de mis decisiones y no en su lugar:

- **Bases de código.** Primero diseño la solución (qué scripts hacen falta, sus responsabilidades, contratos y flujo); a partir de ese diseño pido a la IA la base del código, que después reviso, ajusto e integro yo mismo.
- **Motor comercial.** Cuando trabajo con Unity —un motor comercial con muchísima documentación—, pido a la IA lo que necesito según lo que quiero conseguir. La IA me señala las herramientas y los enfoques que necesito y con los que trabajar más rápido; acto seguido reviso la documentación del motor para comprender mejor lo que estoy haciendo y conocer las posibilidades y opciones de esas herramientas. Así acelero la búsqueda sin dejar de entender cada decisión.

---

## Estado y posibles mejoras

**Hecho**

- Menú principal, intro cinemática y nivel completo (sala principal, pasillo, sala 2).
- Caja fuerte con candado numérico y llave; placas de peso con panel y puertas.
- Puzzle de luz reorganizado (emisor, espejos, receptores, compuerta) con sonidos.
- Sonidos cableados en inspector, lluvia y música en bucle.
- Build final para Windows
- Partículas para el puzzle de los haces y para la lluvia.

**Pendiente**

- Me habría gustado que el juego fuese más intuitivo a la hora de solventar los puzzles pero no ha habido tiempo a generar assets que ayuden a esto.
- Mejoraría bastante la experiencia el que todos los elementos del juego tuviesen su propio modelo 3D, pero los que he podido encontrar o no tenían la licencia adecuada que permitiera su uso o rompían por completo la estética del mismo, por lo que al final he preferido dejar mis modelos (que son simples) y formas básicas de Unity.


---

## Sobre los assets de terceros

Todos los assets de terceros utilizados en el proyecto son de licencia CC0.