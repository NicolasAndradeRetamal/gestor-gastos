# Diseño — gestor-gastos

Guía visual y sistema de diseño de la aplicación. Este documento es el
contrato de implementación de la interfaz: paleta, tipografía, tokens
(expresados en Tailwind CSS v4), especificación de componentes y wireframes
textuales de cada vista. El frontend implementa a partir de aquí sin decisiones
visuales pendientes.

Convenciones: nombres de tokens y clases en inglés; todo el copy de UI en
español. Tailwind CSS v4 vía `@tailwindcss/vite`; los tokens viven bajo
`@theme` en la hoja de estilos global del cliente (`frontend/src/assets/main.css`
o equivalente). Mobile first: las clases base apuntan a ~360 px y se escalan
con `sm:` / `md:` / `lg:`.

---

## 1. Identidad

### 1.1 Personalidad visual

Gestor de gastos personales: una herramienta de confianza para un dato
sensible (el dinero propio), no un producto de entretenimiento. La interfaz
debe transmitir **claridad** (los números se leen de un vistazo, sin
ambigüedad), **calma** (nada grita; los acentos de color se reservan para lo
que importa: montos, alertas, acciones principales) y **precisión** (grillas
consistentes, alineación numérica, sin decoración que no informe). Se prioriza
la legibilidad de cifras y la velocidad para registrar un gasto sobre
cualquier ornamento.

Principios de diseño:

1. **Los números mandan.** Los montos son el elemento con más peso visual de
   cada pantalla (tamaño, peso de fuente, alineación); todo lo demás es
   soporte para leerlos rápido y sin errores.
2. **Un acento, un significado.** El color de marca se reserva para acciones
   primarias y estados activos; el color no se usa nunca como único
   indicador de estado (siempre acompañado de texto, icono o forma).
3. **Formularios cortos y con salida clara.** Registrar un gasto es la
   acción más frecuente de la app: campos mínimos, validación inline,
   confirmación visible, sin pasos innecesarios.
4. **Consistencia sobre originalidad.** Un único lenguaje de tarjeta, un
   único patrón de formulario, un único estilo de badge. La app debe sentirse
   como un solo sistema, no como pantallas diseñadas por separado.

### 1.2 Modos de color

**Modo claro y modo oscuro, ambos soportados desde el inicio, con
conmutador manual explícito.** El estado inicial respeta la preferencia del
sistema (`prefers-color-scheme`); a partir de ahí, el usuario puede alternar
el tema con un control siempre accesible desde el menú de usuario
(`ThemeToggle`, §3.10) y su elección se recuerda entre sesiones.

Justificación de fondo: la app se usa a cualquier hora del día para un gasto
puntual, y el modo oscuro es una expectativa estándar en herramientas
financieras actuales; además, expresar toda la paleta con la función
`light-dark()` de CSS dentro de tokens `@theme` de Tailwind 4 no añade
componentes ni estado nuevo al cliente para el *contenido* del tema — cada
color se define una sola vez y los componentes solo usan utilidades
semánticas (`bg-surface`, `text-ink`, …), nunca variantes `dark:` repetidas.
Lo único que se añade es el mecanismo de *override* descrito abajo.

**Patrón elegido: toggle binario claro ↔ oscuro que arranca desde el
sistema** (no un selector de tres estados claro/oscuro/sistema).
Justificación: con solo dos temas definidos, un botón simple con icono
sol/luna cubre el caso de uso completo — elegir el tema y recordarlo — con
menos superficie de interfaz que un menú de tres opciones; encaja mejor con
el alcance y la sobriedad de un proyecto de portafolio. Contrapartida
aceptada: una vez que el usuario elige manualmente, la app deja de seguir
cambios posteriores del sistema en vivo hasta que vuelva a tocar el control
— comportamiento estándar y esperado de un toggle binario.

**Mecanismo:**

- **Persistencia**: `localStorage`, clave `theme`, valores `'light' | 'dark'`.
  Ausencia de la clave significa que el usuario nunca tocó el control, y la
  app sigue el sistema.
- **Estado inicial sin parpadeo ("flash")**: un script mínimo, inline y
  bloqueante en el `<head>` de `index.html` (antes de cargar el bundle de la
  SPA) lee `localStorage.getItem('theme')`; si el valor es `'light'` o
  `'dark'`, fija `document.documentElement.dataset.theme` a ese valor **antes
  del primer paint**. Si no hay valor guardado, no fija el atributo y el
  documento sigue el sistema vía `color-scheme: light dark` en `:root`
  (§2) — sin necesidad de JavaScript adicional para ese caso. Ejemplo:

  ```html
  <!-- index.html, dentro de <head>, antes del script de la SPA -->
  <script>
    (function () {
      var stored = localStorage.getItem('theme');
      if (stored === 'light' || stored === 'dark') {
        document.documentElement.dataset.theme = stored;
      }
    })();
  </script>
  ```

- **Aplicación del override**: el atributo `data-theme="light" | "dark"` en
  `<html>` activa una regla CSS que fija `color-scheme` a un único valor (ver
  bloque en §2), de modo que la función `light-dark()` de cada token resuelve
  siempre a esa rama. Los componentes no cambian su implementación: siguen
  usando solo utilidades semánticas (`bg-surface`, `text-ink`, `border-line`,
  …), nunca variantes `dark:`. Sin el atributo `data-theme`, `color-scheme:
  light dark` en `:root` deja que el navegador siga `prefers-color-scheme` en
  vivo, incluida la actualización automática si el usuario cambia el tema del
  sistema operativo sin haber elegido nunca manualmente.
- **Cambio manual**: al pulsar `ThemeToggle` (§3.10), la lógica de la SPA (un
  composable tipo `useTheme` o el store que el frontend prefiera) determina
  el tema visual activo — lee `data-theme` si ya existe; si no, consulta
  `matchMedia('(prefers-color-scheme: dark)').matches` — aplica el opuesto
  fijando `data-theme` en `<html>` y lo persiste en `localStorage`. Desde ese
  momento el tema queda fijo en la elección explícita del usuario.
- El control vive dentro del **menú de usuario** que se abre desde el avatar,
  con el mismo patrón en escritorio y en móvil (§3.7, §3.10): agrupado con las
  acciones de cuenta, nunca suelto en el layout ni junto al botón de cerrar
  sesión, y tampoco dentro de una vista de "Configuración", que no existe en
  el alcance actual (§4.6).

### 1.3 Paleta de colores

Paleta armónica de base **índigo** (confianza, tecnología financiera) con
acento **esmeralda** (dinero, positivo, disponible) y un neutro **zinc**
(frío, técnico, deja protagonismo a los números). Todos los pares
texto/fondo listados cumplen WCAG AA (≥ 4.5:1 texto normal, ≥ 3:1 texto
grande/iconos); el detalle de contraste está en la sección 7.

| Token | Rol | Claro | Oscuro |
|---|---|---|---|
| `primary` | Acciones principales, enlaces, foco, estados activos de navegación | `#4f46e5` (indigo-600) | `#818cf8` (indigo-400) |
| `primary-strong` | Hover/activo sobre elementos de marca | `#4338ca` (indigo-700) | `#a5b4fc` (indigo-300) |
| `primary-soft` | Fondos sutiles de selección, chips de marca, fila activa de nav | `#e0e7ff` (indigo-100) | `#1e1b4b` (indigo-950) |
| `accent` | Acento monetario: totales destacados, indicador "dentro de presupuesto", icono de categoría por defecto | `#059669` (emerald-600) | `#34d399` (emerald-400) |
| `accent-soft` | Fondo de badges/estado positivo | `#d1fae5` (emerald-100) | `#022c22` (emerald-950) |
| `surface` | Fondo de página | `#fafafa` (neutral-50 / zinc-50) | `#18181b` (zinc-900) |
| `surface-raised` | Tarjetas, modal, dropdown, header | `#ffffff` | `#27272a` (zinc-800) |
| `surface-sunken` | Fondo de input, filas alternas de tabla, skeletons | `#f4f4f5` (zinc-100) | `#3f3f46` (zinc-700) |
| `ink` | Texto principal | `#18181b` (zinc-900) | `#fafafa` (zinc-50) |
| `ink-muted` | Texto secundario, etiquetas, ayuda | `#52525b` (zinc-600) | `#a1a1aa` (zinc-400) |
| `line` | Bordes, divisores | `#e4e4e7` (zinc-200) | `#3f3f46` (zinc-700) |
| `success` | Confirmaciones, toasts de éxito | `#059669` (emerald-600) | `#34d399` (emerald-400) |
| `success-soft` | Fondo de toast/alerta de éxito | `#d1fae5` (emerald-100) | `#022c22` (emerald-950) |
| `danger` | Errores, borrar, gasto que excede filtro, campos inválidos | `#dc2626` (red-600) | `#f87171` (red-400) |
| `danger-soft` | Fondo de alerta/toast de error, halo de input inválido | `#fee2e2` (red-100) | `#450a0a` (red-950) |
| `warning` | Avisos no bloqueantes (p. ej. categoría con gastos al intentar borrar) | `#d97706` (amber-600) | `#fbbf24` (amber-400) |
| `warning-soft` | Fondo de alerta de aviso | `#fef3c7` (amber-100) | `#451a03` (amber-950) |
| `info` | Mensajes informativos neutros (toasts informativos) | `#0891b2` (cyan-600) | `#22d3ee` (cyan-400) |
| `info-soft` | Fondo de toast informativo | `#cffafe` (cyan-100) | `#083344` (cyan-950) |

`success`/`accent` comparten valor (esmeralda): en esta app "positivo" y
"dinero" son el mismo concepto, así que un solo tono cubre ambos roles sin
inflar la paleta con un verde adicional casi idéntico.

Contraste verificado (WCAG AA): `ink` sobre `surface`/`surface-raised` ≥ 15:1
en ambos modos; `ink-muted` sobre `surface-raised` 7.1:1 claro / 7.6:1 oscuro;
`primary` como texto sobre `surface`/`surface-raised` 5.5:1 claro / 8.1:1
oscuro; `accent`/`success` como texto 4.9:1 claro / 9.0:1 oscuro; `danger`
como texto 5.1:1 claro / 6.8:1 oscuro; `warning` como texto 3.2:1 (solo usado
en tamaño ≥ 18px / semibold o junto a icono, nunca en texto pequeño de
cuerpo). Ningún color de marca se usa como texto pequeño sobre su propio
`-soft` (p. ej. `primary` sobre `primary-soft` es 3.6:1, por debajo de AA
normal: se reserva para iconos/texto grande o se usa `ink`/`primary-strong`
sobre `-soft`).

#### Paleta categórica (gráficos y badges de categoría)

Ocho colores para las categorías predefinidas, alineados con el campo
`color` (`#RRGGBB`) que expone la API. Elegidos para distinguirse entre sí en
un gráfico de dona/barras (incluida una revisión razonable para
protanopia/deuteranopia: alternan cálido/frío y difieren en luminosidad, no
solo en matiz) y para servir como fondo de un punto (`dot`) de 8-10 px o de un
`ring` de badge sin depender de contraste texto-sobre-color:

| Categoría (seed) | Hex | Nombre de referencia |
|---|---|---|
| Comida | `#F97316` | orange-500 |
| Transporte | `#3B82F6` | blue-500 |
| Vivienda | `#8B5CF6` | violet-500 |
| Salud | `#F43F5E` | rose-500 |
| Ocio | `#EAB308` | yellow-500 |
| Servicios | `#06B6D4` | cyan-500 |
| Educación | `#10B981` | emerald-500 |
| Otros | `#71717A` | zinc-500 |

Uso: estos hex son los valores que trae cada categoría desde la base de datos
(predefinidas por seed, personalizadas elegidas por el usuario en un selector
de color limitado a esta paleta — ver §5, `CategoryForm`). Se pintan
**siempre igual en claro y oscuro** (no llevan variante `light-dark()`):
como son colores decorativos de identificación, no de texto, no necesitan
adaptarse al modo; donde se usan como fondo de texto (badge sólido) se valida
contraste individualmente en §5.7 usando `ink` o blanco fijo según el color.
El selector de color al crear/editar una categoría personalizada se limita a
estos 8 valores (más los ya en uso) para garantizar que todo gráfico y badge
de la app permanezca legible y armónico; no es un color picker libre.

### 1.4 Tipografía

**Familia:** [Plus Jakarta Sans](https://fonts.google.com/specimen/Plus+Jakarta+Sans)
(Google Fonts, variable, licencia OFL) para toda la interfaz, incluidos los
montos. Es una geométrica humanista con cifras tabulares nativas
(`font-feature-settings: "tnum" 1`), ideal para una app donde los números se
comparan entre filas. Un solo family evita el coste de una segunda descarga y
mantiene consistencia entre títulos, cuerpo y cifras.

Pesos cargados: `400` (regular), `500` (medium), `600` (semibold), `700`
(bold), `800` (extrabold, reservado para el monto total del dashboard).
Fallback: `"Plus Jakarta Sans", ui-sans-serif, system-ui, -apple-system,
"Segoe UI", sans-serif`.

Todas las cifras monetarias y de fecha usan `tabular-nums` (utilidad de
Tailwind) para que los dígitos no salten al filtrar o actualizar datos.

Escala tipográfica:

| Rol | Clases Tailwind | Uso |
|---|---|---|
| Monto hero (total del dashboard) | `text-4xl sm:text-5xl font-extrabold tracking-tight tabular-nums` | Total del periodo en el dashboard |
| Monto en tarjeta/fila | `text-lg font-bold tabular-nums` | Importe de un gasto en lista, total por categoría |
| Monto pequeño | `text-sm font-semibold tabular-nums` | Importe en badges, resúmenes secundarios |
| H1 (título de vista) | `text-2xl font-bold tracking-tight` | Encabezado de cada página (`Dashboard`, `Gastos`, `Categorías`) |
| H2 (sección) | `text-lg font-semibold` | Encabezado de tarjeta/sección (`Gastos por categoría`) |
| H3 | `text-base font-semibold` | Subtítulos dentro de tarjeta |
| H4 / etiqueta de grupo | `text-xs font-semibold text-ink-muted uppercase tracking-wide` | Encabezados de columna, agrupadores |
| Body | `text-base font-normal leading-relaxed` | Texto de párrafo, descripciones |
| Body en formulario/tabla | `text-sm font-normal` | Inputs, celdas, ítems de lista |
| Small / ayuda | `text-xs text-ink-muted` | Texto de ayuda, timestamps, notas de gasto |
| Botón | `text-sm font-semibold` | Etiqueta de todos los botones |

---

## 2. Tokens de diseño (Tailwind CSS v4, `@theme`)

Bloque a incorporar en la hoja de estilos global del cliente (importa
Tailwind y define los tokens semánticos; no requiere `tailwind.config`
adicional para el tema):

```css
@import "tailwindcss";

@theme {
  /* Font */
  --font-sans: "Plus Jakarta Sans", ui-sans-serif, system-ui, -apple-system,
    "Segoe UI", sans-serif;

  /* Surfaces */
  --color-surface: light-dark(#fafafa, #18181b);
  --color-surface-raised: light-dark(#ffffff, #27272a);
  --color-surface-sunken: light-dark(#f4f4f5, #3f3f46);

  /* Text */
  --color-ink: light-dark(#18181b, #fafafa);
  --color-ink-muted: light-dark(#52525b, #a1a1aa);

  /* Lines */
  --color-line: light-dark(#e4e4e7, #3f3f46);

  /* Brand */
  --color-primary: light-dark(#4f46e5, #818cf8);
  --color-primary-strong: light-dark(#4338ca, #a5b4fc);
  --color-primary-soft: light-dark(#e0e7ff, #1e1b4b);

  /* Accent (money / positive) */
  --color-accent: light-dark(#059669, #34d399);
  --color-accent-soft: light-dark(#d1fae5, #022c22);

  /* Feedback */
  --color-success: light-dark(#059669, #34d399);
  --color-success-soft: light-dark(#d1fae5, #022c22);
  --color-danger: light-dark(#dc2626, #f87171);
  --color-danger-soft: light-dark(#fee2e2, #450a0a);
  --color-warning: light-dark(#d97706, #fbbf24);
  --color-warning-soft: light-dark(#fef3c7, #451a03);
  --color-info: light-dark(#0891b2, #22d3ee);
  --color-info-soft: light-dark(#cffafe, #083344);

  /* Categorical chart palette (fixed, no light-dark) */
  --color-cat-1: #f97316; /* Comida */
  --color-cat-2: #3b82f6; /* Transporte */
  --color-cat-3: #8b5cf6; /* Vivienda */
  --color-cat-4: #f43f5e; /* Salud */
  --color-cat-5: #eab308; /* Ocio */
  --color-cat-6: #06b6d4; /* Servicios */
  --color-cat-7: #10b981; /* Educación */
  --color-cat-8: #71717a; /* Otros */

  /* Radius */
  --radius-sm: 0.375rem; /* 6px  — badges, inputs pequeños */
  --radius-md: 0.5rem;   /* 8px  — inputs, botones */
  --radius-lg: 0.75rem;  /* 12px — tarjetas, filas de tabla en móvil */
  --radius-xl: 1rem;     /* 16px — modal, tarjeta destacada del dashboard */
  --radius-full: 9999px; /* pills, badges, avatar, botón circular */

  /* Shadow */
  --shadow-card: 0 1px 2px 0 rgb(0 0 0 / 0.04), 0 1px 3px 0 rgb(0 0 0 / 0.06);
  --shadow-raised: 0 4px 6px -1px rgb(0 0 0 / 0.08), 0 2px 4px -2px rgb(0 0 0 / 0.06);
  --shadow-modal: 0 20px 25px -5px rgb(0 0 0 / 0.15), 0 8px 10px -6px rgb(0 0 0 / 0.1);
}

:root {
  color-scheme: light dark;
}

/* Override manual de tema: fijado por ThemeToggle (§3.10) vía data-theme
   en <html>. Sin el atributo, se sigue prefers-color-scheme (regla de arriba). */
:root[data-theme='light'] {
  color-scheme: light;
}

:root[data-theme='dark'] {
  color-scheme: dark;
}

body {
  @apply bg-surface text-ink antialiased;
  font-feature-settings: "tnum" 1;
}
```

Estilo de foco global (misma hoja, fuera de `@theme`):

```css
@layer base {
  :focus-visible {
    @apply outline-2 outline-offset-2 outline-primary rounded-sm;
  }
}
```

Uso: solo utilidades semánticas (`bg-surface-raised`, `text-ink-muted`,
`border-line`, `text-primary`, `bg-danger-soft`, …) en componentes — nunca
colores crudos de la paleta base de Tailwind (`bg-white`, `text-zinc-500`).
Excepción explícita: la paleta categórica (`--color-cat-*`) y el `color` hex
que trae cada categoría desde la API, que se usan tal cual (son datos, no
tema).

### Espaciado y breakpoints

Se usa la escala de espaciado por defecto de Tailwind (base 4px), sin
extenderla. Ritmo estándar de la app:

| Uso | Clase |
|---|---|
| Gap entre elementos de una fila densa (badge + texto) | `gap-2` (8px) |
| Padding interno de tarjeta en móvil | `p-4` (16px) |
| Padding interno de tarjeta en `sm:` y superior | `sm:p-6` (24px) |
| Separación entre secciones de una vista | `space-y-6` (24px) |
| Separación entre campos de un formulario | `space-y-4` (16px) |
| Padding de contenedor de página | `px-4 py-6 sm:px-6 lg:px-8` |

Breakpoints (por defecto de Tailwind): `sm` 640px, `md` 768px (activa
sidebar de escritorio y grillas de 2 columnas), `lg` 1024px (grillas de
dashboard a 3 columnas, ancho máximo de contenido). Ancho máximo de contenido:
`max-w-6xl mx-auto` en el contenedor de página.

---

## 3. Componentes base

Reglas transversales: todo elemento interactivo tiene área táctil ≥ 44×44px,
usa la regla de foco global (§2) y nunca `outline-none` sin reemplazo. Los
cinco estados obligatorios de cada componente interactivo son *reposo, hover,
focus, activo/presionado, deshabilitado*; se añade *carga* donde aplica
(botones de envío, listas con datos remotos).

### 3.1 Botones

Base común: `inline-flex items-center justify-center gap-2 h-11 rounded-md px-4 text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-50`.
Altura de 44px cumple el objetivo táctil mínimo. Variante compacta
(`h-9 px-3 text-sm`) solo dentro de tablas/toolbars donde el espacio vertical
es limitado, nunca como único control de una acción destructiva importante.

| Variante | Reposo | Hover | Focus | Activo | Deshabilitado | Carga |
|---|---|---|---|---|---|---|
| **Primario** (`Guardar`, `Añadir gasto`, `Iniciar sesión`) | `bg-primary text-white` | `hover:bg-primary-strong` | anillo global + `bg-primary-strong` | `active:scale-[0.98]` | `disabled:opacity-50` (sin cambiar color) | reemplaza el texto por `Spinner` (§3.9) a `size-4` + texto `sr-only` con la acción en curso (`Guardando…`); `aria-busy="true"`, el botón queda `disabled` |
| **Secundario** (`Cancelar`, `Ver más`, acciones no primarias) | `bg-surface-raised text-ink border border-line` | `hover:bg-surface-sunken` | anillo global | `active:scale-[0.98]` | `disabled:opacity-50` | igual patrón que primario, spinner en `text-ink-muted` |
| **Peligro** (`Eliminar gasto`, `Eliminar categoría`) | `bg-danger text-white` | `hover:bg-red-700` (10% más oscuro) | anillo global (`outline-danger` en vez de `outline-primary` para este botón) | `active:scale-[0.98]` | `disabled:opacity-50` | igual patrón, spinner en `text-white` |
| **Fantasma / texto** (acciones terciarias, `Editar`, enlaces de tabla) | `text-primary font-semibold` sin fondo | `hover:bg-primary-soft` (padding propio `px-2 py-1 rounded-md`) | anillo global | `active:scale-[0.98]` | `disabled:opacity-50 disabled:hover:bg-transparent` | spinner inline `size-4` reemplazando el icono si lo tiene |

Icono opcional a la izquierda del texto, `size-4` (16px, `aria-hidden="true"`),
el texto siempre visible salvo en botones **icon-only** (p. ej. cerrar modal,
acciones de tabla), que llevan `aria-label` explícito en vez de texto visible.

**Botón icon-only — tamaño por defecto.** Área táctil `size-11` (44×44px,
`px-0 shrink-0`) con icono **`size-6` (24px)** centrado. Es el tamaño por
defecto de todo botón de solo icono de la app: cerrar modal (§3.6), cerrar
toast (§3.8) y cualquier acción de icono aislada del layout o la navegación.

Decisión de tamaño (número verificable): el glifo por defecto de un icon-only
es **24px (`size-6`)** dentro del área de **44px (`size-11`)**, dejando ~10px
de aire por lado. Una versión previa usaba 20px (`size-5`), que dentro de los
mismos 44px dejaba ~12px por lado y se percibía pequeño para acciones
importantes; por eso el mínimo por defecto se sube a 24px. Un icono más
pequeño dentro de esa misma área de 44px es un defecto de diseño, no un
detalle — el objetivo no es solo el área de toque sino que el icono se perciba
con claridad a distancia de lectura. Nota: la acción **cerrar sesión** ya no
es un botón icon-only en ninguna parte de la app; vive como fila con etiqueta
dentro del menú de usuario (§3.7), donde su icono es `size-5` (20px) y va
siempre acompañado del texto `Cerrar sesión`.

**Botón icon-only — variante compacta (excepción documentada).** Área `size-8`
(32px) con icono `size-4` (16px), **únicamente** para acciones repetidas por
fila dentro de tablas y toolbars densos de escritorio (`md:` y superior),
donde varias acciones por fila compiten por espacio horizontal — ver
`Editar`/`Eliminar` en la tabla de gastos, §3.4. Es una excepción consciente
y acotada al mínimo general de 44×44px (§5), no la regla general: se justifica
por densidad visual en un contexto exclusivo de escritorio, ya que la vista
equivalente en móvil usa tarjetas apiladas con toda la fila como área de
toque (§3.4), donde el objetivo táctil de 44px se sigue cumpliendo. Esta
variante no se usa para una acción aislada, una acción destructiva sin
alternativa, ni en ningún control de navegación o layout persistente
(sidebar, topbar, tab bar, `ThemeToggle`).

### 3.2 Inputs y selects de formulario

Estructura: `<div>` contenedor → `<label>` visible arriba → control → texto de
ayuda/error debajo.

- **Label**: `block text-sm font-medium text-ink mb-1.5`. Siempre visible
  (nunca placeholder-como-label). Campo requerido: sufijo `text-danger`
  `*` con `aria-hidden` (el `required` real va en el control).
- **Input de texto/número/fecha**: `w-full h-11 rounded-md border border-line bg-surface-raised px-3 text-sm text-ink placeholder:text-ink-muted transition-colors`.
  - Hover: `hover:border-ink-muted`.
  - Focus: anillo global + `border-primary`.
  - Deshabilitado: `disabled:bg-surface-sunken disabled:text-ink-muted disabled:cursor-not-allowed`.
  - Inválido: `border-danger` + anillo de foco `outline-danger` en vez de
    `outline-primary` cuando recibe foco estando inválido; `aria-invalid="true"`
    y `aria-describedby` apuntando al mensaje de error.
- **Select** (categoría, ordenar): mismas clases que el input de texto más
  icono de chevron a la derecha (`absolute right-3 pointer-events-none
  text-ink-muted size-4`); mismos estados.
- **Textarea** (nota del gasto): igual que input, `min-h-20 py-2 resize-y`.
- **Selector de color de categoría**: fila de 8 círculos `size-8 rounded-full`
  (uno por color de la paleta categórica, §1.3) dentro de un `role="radiogroup"`;
  el seleccionado lleva `ring-2 ring-offset-2 ring-primary` y un check blanco
  centrado (`size-4`) además del `aria-checked`. El color nunca es el único
  indicador de selección.
- **Texto de ayuda**: `mt-1.5 text-xs text-ink-muted`.
- **Mensaje de error de campo**: `mt-1.5 flex items-center gap-1 text-xs font-medium text-danger`,
  con icono de alerta `size-3.5` (`aria-hidden`) + el mensaje devuelto por la
  API (`errors.amount[0]`, ya en español) o la validación equivalente en
  cliente. `role="alert"` en el contenedor del mensaje para que se anuncie al
  aparecer.
- **Checkbox / radio nativo** (si se necesitan, p. ej. futuros filtros):
  `size-5 rounded border-line text-primary` con foco global.

### 3.3 Tarjetas (cards)

Base: `rounded-xl border border-line bg-surface-raised p-4 shadow-card sm:p-6`.
Es el contenedor de toda pieza de contenido agrupado: tarjetas del dashboard,
formulario dentro de modal, tarjeta de resumen. `shadow-card` es sutil a
propósito; en modo oscuro el `border` hace la mayor parte del trabajo de
separación (las sombras casi no se perciben sobre fondos oscuros).

- **Tarjeta con encabezado y acción** (p. ej. "Gastos por categoría" con un
  selector de rango): `flex items-center justify-between gap-2 mb-4` como fila
  de cabecera, `H2` a la izquierda, control secundario a la derecha.
- **Tarjeta interactiva** (si en el futuro una tarjeta completa es
  clickeable): añade `transition-shadow hover:shadow-raised cursor-pointer` y
  se implementa como `<button>`/`<a>`, nunca `onClick` sobre un `<div>`.

### 3.4 Tabla / lista de gastos

Patrón responsivo: **tabla real en `md:` y superior, lista de tarjetas
apiladas en móvil** (una tabla angosta con 4+ columnas no es legible en 360px).

**Móvil** (`< md`) — cada gasto es una fila-tarjeta:
`flex items-center justify-between gap-3 rounded-lg border border-line bg-surface-raised px-4 py-3`,
apiladas con `space-y-2` dentro de un `<ul>`:

- Izquierda: `CategoryBadge` (§3.5, solo el punto de color + icono, `size-8`
  circular) + columna con nota/categoría (`text-sm font-medium text-ink`, la
  nota si existe, si no el nombre de categoría) sobre fecha
  (`text-xs text-ink-muted`, `dd MMM` es-ES).
- Derecha: monto `text-base font-bold tabular-nums text-ink` sobre el nombre
  de categoría en `text-xs text-ink-muted` cuando la nota ocupó la línea
  principal.
- Toda la fila es un `<button>` que abre el detalle/edición (§3.6 modal); el
  swipe-to-delete queda fuera de alcance del MVP (se borra desde el modal de
  edición).

**Escritorio** (`md:` y superior) — `<table class="w-full text-sm">` dentro de
la tarjeta contenedora:

- `<thead>`: fila `border-b border-line`, celdas `H4` (`text-xs font-semibold
  text-ink-muted uppercase tracking-wide px-4 py-3 text-left`), columnas
  `Fecha | Categoría | Nota | Monto | Acciones` (`Monto` alineado a la derecha
  con `text-right`, `Acciones` centrada).
- `<tbody>`: filas `border-b border-line last:border-0
  hover:bg-surface-sunken transition-colors`, celdas `px-4 py-3`.
  - Fecha: `tabular-nums text-ink-muted`.
  - Categoría: `CategoryBadge` (§3.5) tamaño `sm`.
  - Nota: `text-ink`, `truncate max-w-xs`; `—` en `text-ink-muted` si no hay nota.
  - Monto: `text-right font-semibold tabular-nums text-ink`.
  - Acciones: botones fantasma icon-only `Editar` (lápiz) y `Eliminar`
    (papelera, variante peligro fantasma: `text-danger hover:bg-danger-soft`),
    variante compacta de §3.1 (`size-8`, icono `size-4`) por ser acciones
    repetidas por fila en una tabla de escritorio, con `aria-label`.
- Paginación bajo la tabla: `flex items-center justify-between mt-4 text-sm text-ink-muted`,
  texto `Mostrando {from}–{to} de {totalItems}` a la izquierda, botones
  secundarios compactos `Anterior` / `Siguiente` (`disabled` en los extremos)
  a la derecha.

### 3.5 Badge de categoría (`CategoryBadge`)

Chip que representa una categoría en toda la app (lista de gastos, filtros,
gestión de categorías, leyenda de gráficos). Usa el `color` hex propio de la
categoría (dato de API), no un token de tema.

- **Tamaño `md`** (por defecto, filtros y gestión): `inline-flex items-center gap-1.5 h-7 rounded-full pl-1.5 pr-2.5 text-xs font-medium`,
  fondo `color-mix(in srgb, {color} 15%, transparent)` sobre `surface-raised`,
  texto `{color}` si su contraste sobre ese fondo mezclado ≥ 4.5:1 (todos los
  8 colores de la paleta categórica lo cumplen calculados sobre `surface`
  claro y oscuro); punto sólido a la izquierda `size-2 rounded-full` con
  `background-color: {color}`.
- **Tamaño `sm`** (dentro de tabla/lista densa): solo el punto `size-2.5
  rounded-full` + nombre en `text-ink` (sin fondo de chip) — evita ruido
  visual en filas repetidas.
- El nombre de la categoría siempre acompaña al color (nunca un punto de
  color solo, salvo en el punto `sm` de tabla donde el nombre está en la
  celda contigua).
- Categoría predefinida (`isDefault: true`): sin marca adicional (son la
  mayoría de las categorías vistas). Si se necesita distinguirlas en la vista
  de gestión, usa el texto de ayuda `Predefinida`, no un color distinto.

### 3.6 Modal / diálogo

Usado para alta/edición de gasto, alta/edición de categoría y confirmación de
borrado. Un solo modal a la vez.

- **Overlay**: `fixed inset-0 bg-zinc-950/50 backdrop-blur-[1px]`, cierra al
  hacer click fuera del panel (excepto en confirmaciones destructivas, que
  requieren una respuesta explícita).
- **Panel**: `w-full max-w-md rounded-xl bg-surface-raised p-6 shadow-modal`,
  centrado (`fixed inset-0 flex items-center justify-center p-4` en el
  wrapper); en móvil ocupa el ancho disponible con margen de `p-4` alrededor
  (nunca pantalla completa sin bordes, para conservar la sensación de
  diálogo).
- **Cabecera**: `flex items-center justify-between mb-4` — título `H2` +
  botón cerrar icon-only (`×`, tamaño por defecto de §3.1 — `size-11`,
  icono `size-6`, `aria-label="Cerrar"`).
- **Cuerpo**: formulario en `space-y-4` (alta/edición) o texto de
  confirmación `text-sm text-ink-muted` (borrado).
- **Pie**: `flex items-center justify-end gap-3 mt-6` — botón secundario
  `Cancelar` + botón primario o peligro según la acción, en ese orden
  (cancelar primero, para que la acción destructiva no quede donde el pulgar
  suele tocar por hábito).
- **Foco**: al abrir, foco al primer campo (o al título en confirmaciones);
  trampa de foco dentro del panel (`Tab`/`Shift+Tab` ciclan dentro); `Escape`
  cierra (salvo mientras se envía, `aria-busy`); al cerrar, el foco vuelve al
  elemento que abrió el modal. `role="dialog"` + `aria-modal="true"` +
  `aria-labelledby` apuntando al título.
- **Confirmación de borrado de categoría con gastos asociados** (`409` de la
  API): el modal de confirmación pasa a un estado de aviso —
  `bg-warning-soft border border-warning/30 rounded-lg p-3` con icono de
  alerta y el texto `Esta categoría tiene gastos asociados. Reasigna o
  elimina esos gastos antes de borrarla.`; el botón de acción principal se
  deshabilita (no hay acción posible sin resolver eso primero).

### 3.7 Navegación

**Escritorio (`md:` y superior)** — sidebar fijo a la izquierda,
`w-64 shrink-0 border-r border-line bg-surface-raised flex flex-col h-dvh sticky top-0`:

- Cabecera del sidebar: nombre de la app `text-lg font-bold text-ink` +
  icono de marca `size-6 text-primary`, `h-16 flex items-center px-6 border-b border-line`.
- Enlaces de navegación (`Dashboard`, `Gastos`, `Categorías`), cada uno
  `flex items-center gap-3 h-11 rounded-md px-3 mx-3 text-sm font-medium text-ink-muted transition-colors hover:bg-surface-sunken`
  con icono `size-5` a la izquierda; el enlace activo (`router-link-exact-active`):
  `bg-primary-soft text-primary font-semibold` + una barra izquierda de 3px
  `bg-primary` (el color nunca es el único indicador: el `aria-current="page"`
  y el peso de fuente también cambian).
- Pie del sidebar: **botón que abre el menú de usuario** (mismo componente
  que en móvil, ver "Menú de usuario" abajo). El disparador ocupa el ancho del
  pie —
  `mt-auto border-t border-line p-3 flex w-full items-center gap-3 rounded-none hover:bg-surface-sunken transition-colors`—
  y contiene: avatar circular con iniciales
  (`size-9 rounded-full bg-primary-soft text-primary font-semibold flex items-center justify-center text-sm shrink-0`),
  `displayName` (`text-sm font-medium truncate text-left flex-1`) e icono
  `chevron` (`size-5 text-ink-muted shrink-0`, `aria-hidden`) que indica que
  despliega un menú. `aria-haspopup="menu"` + `aria-expanded`. El menú aparece
  **hacia arriba** (anclado al pie del sidebar) porque el disparador está en el
  borde inferior. Ni `Cerrar sesión` ni `ThemeToggle` viven ya sueltos en el
  pie: ambos son filas dentro de este menú.

**Menú de usuario (`UserMenu`) — contenedor de acciones de cuenta.** Patrón
unificado en escritorio y móvil; es el estándar de la industria (Gmail,
GitHub, Linear, Notion agrupan perfil, ajustes, tema y salir bajo el avatar).
Se elige frente a una topbar de escritorio dedicada porque en `md:` la app no
tiene topbar —solo sidebar—, e introducir una barra superior solo para alojar
el conmutador añadiría cromo sin contenido; el avatar del pie del sidebar ya es
el lugar convencional de las acciones de cuenta. Popover
`w-56 rounded-md border border-line bg-surface-raised p-1 shadow-raised`,
`role="menu"`, con trampa/retorno de foco y cierre por `Escape` o click fuera:

1. **Encabezado no interactivo**: `displayName` (`px-3 py-2 text-sm font-medium
   text-ink truncate`) y, si existe, el correo debajo (`text-xs text-ink-muted
   truncate`). `role="presentation"`.
2. **Fila `ThemeToggle`** (§3.10, variante `menu-item`): icono sol/luna
   `size-5` a la izquierda + texto `Modo claro` / `Modo oscuro`.
3. **Divisor** `my-1 border-t border-line` — separa el ajuste de apariencia de
   la acción destructiva, para que `Cerrar sesión` no quede pegado a un control
   que se pulsa a menudo.
4. **Fila `Cerrar sesión`**: `role="menuitem"`,
   `flex h-11 w-full items-center gap-2 rounded-md px-3 text-sm font-medium
   text-ink-muted hover:bg-surface-sunken`, icono `logout` `size-5` a la
   izquierda + texto. Es la última fila y la única destructiva.

Cada fila del menú mide `h-11` (44px) como objetivo táctil; los iconos de fila
son `size-5` (20px) **con** texto visible, no icon-only.

**Móvil (`< md`)** — barra inferior fija de navegación (los tres destinos
principales) + topbar simple:

- Topbar: `h-14 flex items-center justify-between px-4 border-b border-line bg-surface-raised sticky top-0 z-10` —
  nombre de la vista actual (`H1` reducido a `text-lg font-bold`) a la
  izquierda, y a la derecha el avatar del usuario (`size-9`, botón con
  `aria-haspopup="menu"`) que abre el **mismo `UserMenu`** descrito arriba
  (encabezado + `ThemeToggle` + divisor + `Cerrar sesión`), anclado hacia
  abajo desde el avatar.
- Tab bar inferior: `fixed inset-x-0 bottom-0 z-10 flex border-t border-line bg-surface-raised pb-[env(safe-area-inset-bottom)]`,
  tres botones iguales (`Dashboard`, `Gastos`, `Categorías`)
  `flex-1 flex flex-col items-center justify-center gap-0.5 h-16 text-xs font-medium text-ink-muted`
  con icono `size-5` arriba y etiqueta abajo; el activo:
  `text-primary` + icono relleno en vez de contorno (variación de forma, no
  solo de color) + `aria-current="page"`. El contenido de página añade
  `pb-16` para no quedar oculto tras la barra.
- El botón flotante de "Añadir gasto" (acción más frecuente) vive **dentro**
  de la vista de Gastos, no en la tab bar (ver §4.4), para no competir con la
  navegación entre secciones.

### 3.8 Toasts / notificaciones

Contenedor: `fixed top-4 inset-x-4 z-50 flex flex-col items-center gap-2
sm:inset-x-auto sm:right-4 sm:items-end` (apiladas, la más nueva abajo,
máximo 3 visibles). Cada toast:

`flex items-start gap-3 w-full sm:w-96 rounded-lg border p-4 shadow-raised`,
con icono `size-5 shrink-0` (`aria-hidden`) + texto `text-sm text-ink flex-1` +
botón cerrar icon-only opcional, tamaño por defecto de §3.1 (`size-11`, icono
`size-6`; al ser una acción independiente y no una fila de tabla, no aplica
la variante compacta). `role="status"` (éxito/info) o
`role="alert"` (error/warning) para que el lector de pantalla lo anuncie sin
esperar foco. Auto-descarta a los 5s (éxito/info) o permanece hasta que el
usuario lo cierre (error).

| Tipo | Clases de color | Icono |
|---|---|---|
| Éxito (`Gasto guardado`) | `bg-success-soft border-success/30 text-ink` | check en círculo, `text-success` |
| Error (`No se pudo guardar el gasto`) | `bg-danger-soft border-danger/30 text-ink` | alerta en círculo, `text-danger` |
| Aviso | `bg-warning-soft border-warning/30 text-ink` | triángulo de alerta, `text-warning` |
| Info | `bg-info-soft border-info/30 text-ink` | "i" en círculo, `text-info` |

### 3.9 Estados vacíos, error y carga

**Estado vacío** (`EmptyState`, reutilizable): columna centrada dentro de la
tarjeta contenedora — `flex flex-col items-center gap-3 rounded-xl border border-dashed border-line px-6 py-12 text-center`,
icono `size-10 text-ink-muted` (`aria-hidden`), título `text-sm font-semibold text-ink`,
descripción `text-sm text-ink-muted`, y opcionalmente un botón primario de
acción (`+ Añadir gasto`, `+ Crear categoría`) debajo.

- Sin gastos en el rango filtrado: `Sin gastos en este periodo` /
  `Ajusta el rango de fechas o el filtro de categoría, o añade un gasto
  nuevo.` + botón `Añadir gasto`.
- Sin categorías propias (vista Categorías): `Aún no creaste categorías
  propias` / `Usa las predefinidas o crea una para tus gastos habituales.` +
  botón `Crear categoría`.
- Dashboard sin datos históricos: `Todavía no hay datos para mostrar` /
  `Registra tu primer gasto para ver el resumen.` + botón `Añadir gasto`.

**Estado de error** (fallo de red/API al cargar una vista, no de un campo de
formulario — ese va en §3.2): misma estructura que `EmptyState` pero
`border-danger/30 bg-danger-soft` en vez de borde punteado, icono de alerta
`text-danger`, texto `No se pudo cargar la información. Comprueba tu conexión
e inténtalo de nuevo.`, botón secundario `Reintentar`. `role="alert"`.

**Estados de carga**:

- **Spinner**: SVG `animate-spin`, círculo a `opacity-25` + arco de 90° a
  opacidad plena, `stroke="currentColor" stroke-width="2.5" fill="none"`,
  tamaño heredado (`size-4` en botones, `size-8` como bloqueo de vista).
  Siempre con `role="status"` + `sr-only` (`Cargando…`) o `aria-hidden` si un
  texto hermano ya lo anuncia.
- **Skeleton** (primera carga de listas/tarjetas con datos remotos): bloques
  `animate-pulse rounded-md bg-surface-sunken motion-reduce:animate-none`
  replicando la geometría real del contenido — filas de tabla, tarjetas de
  gráfico (`h-64` para reservar el alto exacto del chart y evitar salto de
  layout), tarjeta de resumen del dashboard. Envueltos en `role="status"` con
  `sr-only` `Cargando…`.
- **Botones**: patrón de §3.1 (spinner reemplaza el contenido, `aria-busy`,
  se deshabilita mientras carga).

### 3.10 Conmutador de tema (`ThemeToggle`)

Control que alterna entre modo claro y modo oscuro (mecanismo completo en
§1.2). Se renderiza como **fila con etiqueta dentro del menú de usuario**
(`UserMenu`, §3.7), con el mismo patrón visual que la fila `Cerrar sesión`:
`role="menuitem"`, `flex h-11 w-full items-center gap-2 rounded-md px-3
text-sm font-medium`, icono sol/luna `size-5` (20px) a la izquierda + texto.
Esta es su única ubicación, idéntica en escritorio y en móvil. No es un botón
icon-only suelto en el layout: el texto de la fila comunica la acción sin
depender solo del icono.

- **Icono**: sol / luna — dos íconos nuevos a sumar al set de `AppIcon`
  (`sun`, `moon`). El icono mostrado representa el modo **activo** (sol si el
  tema activo es claro, luna si es oscuro), no el modo destino: es la
  convención más frecuente en este tipo de control y la menos ambigua para
  quien no está familiarizado con el patrón.
- **`aria-label`**: dinámico según el modo activo — `Cambiar a modo oscuro`
  cuando el tema activo es claro, `Cambiar a modo claro` cuando el tema
  activo es oscuro (nunca un texto estático como "Cambiar tema", que no
  informa el resultado de la acción).
- **Estados** (los de una fila de menú):

| Estado | Especificación |
|---|---|
| Reposo | `text-ink-muted`, sin fondo (fila del menú) |
| Hover | `hover:bg-surface-sunken` en toda la fila |
| Focus | anillo global (§2) al recorrer el menú con teclado |
| Activo/presionado | `active:scale-[0.99]` |
| Deshabilitado | no aplica — el control está siempre disponible |
| Carga | no aplica — el cambio de tema es síncrono, sin espera de red |

- **Ubicación** (unificada, un solo patrón): fila dentro del menú de usuario
  (`UserMenu`, §3.7) que se abre desde el avatar. Es idéntica en ambos tamaños:
  - **Escritorio** (`md:` y superior): el avatar del pie del sidebar abre el
    menú hacia arriba; `ThemeToggle` es la primera fila del menú, encima del
    divisor y de `Cerrar sesión`.
  - **Móvil** (`< md`): el avatar de la topbar abre el mismo menú hacia abajo;
    `ThemeToggle` ocupa la misma posición.
  - En ambos casos la fila muestra el icono sol/luna a la izquierda y el texto
    `Modo claro` / `Modo oscuro` (el modo al que cambia al pulsar, coherente
    con el texto de acción del resto del menú).
- El estado nunca depende únicamente del color: el icono cambia de forma
  (sol ↔ luna) y el `aria-label`/texto visible cambian junto con él.

### 3.11 Barra de progreso de presupuesto (`BudgetProgress`)

Muestra el gasto del mes en curso frente al límite de una categoría, con un
estado visual que nunca depende solo del color. Se usa en el Dashboard (§4.3) y
en la vista de presupuestos (§4.7).

Anatomía (de arriba a abajo), dentro de una fila o tarjeta:

1. **Cabecera** `flex items-center justify-between gap-2`: a la izquierda un
   `CategoryBadge` `sm` (punto de color + nombre); a la derecha el importe en
   `tabular-nums text-sm`: `{gastado} / {límite}` (`text-ink` el gastado,
   `text-ink-muted` el límite).
2. **Riel** `mt-2 h-2.5 w-full rounded-full bg-surface-sunken overflow-hidden`
   con una **barra interior** `h-full rounded-full transition-[width]
   duration-300` cuyo ancho es `min(porcentaje, 100)%` y cuyo color depende del
   estado:
   - `ok` (< 80%): `bg-success`.
   - `warning` (80–100%): `bg-warning`.
   - `exceeded` (> 100%): `bg-danger`, y el riel completo se pinta
     `bg-danger-soft` para reforzar el desborde.
3. **Pie** `mt-1 flex items-center gap-1 text-xs`: icono + texto de estado (el
   color acompaña, no sustituye, al texto):
   - `ok`: sin icono, `text-ink-muted`, texto `{porcentaje}% usado`.
   - `warning`: icono `alert-triangle` `size-3.5 text-warning`, texto
     `text-warning font-medium` `Te estás acercando al límite ({porcentaje}%)`.
   - `exceeded`: icono `alert-circle` `size-3.5 text-danger`, texto
     `text-danger font-medium` `Límite superado ({porcentaje}%)`.

Accesibilidad: el riel lleva `role="progressbar"` con `aria-valuenow`
(porcentaje, sin recortar), `aria-valuemin="0"`, `aria-valuemax="100"` y
`aria-label` `Presupuesto de {categoría}`. El estado se comunica además por
texto (pie) e icono, nunca solo por color.

---

## 4. Layout y vistas

### 4.1 Estructura general

Contenedor raíz: `flex min-h-dvh` — sidebar fijo (§3.7) a la izquierda en
`md:` y superior, columna de contenido a la derecha (`flex-1 min-w-0`). En
móvil, el sidebar no existe; en su lugar, topbar + tab bar inferior (§3.7).

Contenido de cada vista: `max-w-6xl mx-auto px-4 py-6 sm:px-6 lg:px-8 pb-20 md:pb-6`
(el `pb-20` en móvil deja espacio para la tab bar fija). `H1` de la vista al
inicio, seguido de `space-y-6` entre secciones/tarjetas.

Rutas protegidas (todo salvo `/login` y `/register`) exigen sesión; sin token
válido, redirección a `/login`. Rutas de autenticación no muestran sidebar ni
tab bar (layout propio, §4.2).

### 4.2 Login y Registro

Layout compartido, sin sidebar/nav: columna única centrada,
`min-h-dvh flex items-center justify-center p-4 bg-surface`, tarjeta
`w-full max-w-sm` (misma base de tarjeta que §3.3, `p-6 sm:p-8`).

Contenido de la tarjeta (de arriba a abajo):

1. Nombre/logo de la app centrado (`text-xl font-bold text-ink` + icono
   `size-7 text-primary`), `mb-6`.
2. `H2` del formulario: `Iniciar sesión` / `Crear cuenta`.
3. Formulario (`space-y-4`):
   - **Login**: campo `Correo electrónico` (email), campo `Contraseña`
     (password, sin toggle de mostrar/ocultar en el MVP).
   - **Registro**: `Nombre`, `Correo electrónico`, `Contraseña` (ayuda
     `mt-1.5 text-xs text-ink-muted`: `Mínimo 8 caracteres`).
   - Error de credenciales (`401` en login): banner
     `bg-danger-soft border border-danger/30 rounded-lg p-3 text-sm text-danger mb-4`
     encima del formulario, texto `Correo o contraseña incorrectos.`
     (mensaje genérico, igual que especifica el contrato de API).
   - Error de email en uso (`409` en registro): mismo patrón de banner,
     `Ya existe una cuenta con este correo.`
   - Botón primario de ancho completo (`w-full`): `Iniciar sesión` /
     `Crear cuenta`, con estado de carga.
4. Enlace secundario debajo del formulario, centrado, `text-sm text-ink-muted mt-6`:
   `¿No tenés cuenta? Registrate` (en login, `Registrate` como enlace
   `text-primary font-semibold`) / `¿Ya tenés cuenta? Iniciá sesión` (en
   registro).

Tras login/registro exitoso, redirección a `/` (Dashboard).

### 4.3 Dashboard (`/`)

Propósito: vista de aterrizaje tras iniciar sesión; resumen del gasto del
usuario con total, desglose por categoría y evolución mensual.

Layout (mobile first, una columna que pasa a grilla en `lg:`):

1. `H1`: `Resumen` + selector de rango de fechas a la derecha en `md:`
   (debajo del título en móvil, ancho completo): control compacto tipo
   select/botón con opciones predefinidas (`Este mes`, `Últimos 3 meses`,
   `Este año`, `Todo`) — reutiliza los parámetros `from`/`to` de
   `GET /api/dashboard/summary`.
2. **Tarjeta de total** (destacada, ancho completo): fondo distintivo
   `bg-primary-soft border-primary/20` en vez del `surface-raised` genérico,
   para separarla visualmente de las tarjetas de detalle. Contenido:
   etiqueta `Total del periodo` (`H4`) + monto hero (`text-4xl sm:text-5xl
   font-extrabold tabular-nums text-ink`).
3. Grilla de dos tarjetas de gráfico, `grid grid-cols-1 gap-6 lg:grid-cols-2`:
   - **Gasto por categoría**: tarjeta estándar con `H2` `Por categoría` +
     gráfico de dona (Chart.js, colores = `color` de cada categoría de
     `byCategory`) a la izquierda/arriba y leyenda a la derecha/abajo en
     móvil (`flex flex-col md:flex-row items-center gap-6`) — cada ítem de
     leyenda es un `CategoryBadge` `md` con el monto alineado a la derecha
     (`tabular-nums`).
   - **Gasto por mes**: tarjeta estándar con `H2` `Por mes` + gráfico de
     barras (Chart.js, barras en `--color-primary`) usando `byMonth`, eje X
     con mes abreviado es-ES (`ene`, `feb`…).
   - Alto fijo del área de gráfico: `h-64` (evita salto de layout entre
     skeleton y datos).
4. Estado vacío (§3.9) cuando `total === 0` y no hay filtros aplicados,
   reemplazando los tres bloques anteriores.
5. Carga: skeleton de la tarjeta de total (`h-32`) + dos tarjetas de gráfico
   en `animate-pulse` mientras `GET /api/dashboard/summary` está en curso.

Responsive: en `lg:` (≥1024px) las dos tarjetas de gráfico quedan lado a lado;
por debajo, apiladas. El selector de rango pasa de fila completa (móvil) a
alineado junto al `H1` (`md:flex md:items-center md:justify-between`).

### 4.4 Gastos (`/expenses`)

Propósito: listar, filtrar y gestionar (crear/editar/borrar) los gastos del
usuario.

Layout:

1. `H1`: `Gastos` + botón primario `+ Añadir gasto` a la derecha en `md:`
   (en móvil, botón de ancho completo debajo del título, ya que es la acción
   más frecuente de la vista y merece el pulgar disponible en pantallas
   chicas).
2. **Barra de filtros**, dentro de una tarjeta compacta o directamente sobre
   `surface` (`flex flex-col gap-3 sm:flex-row sm:items-end mb-6`):
   - `Desde` / `Hasta` (inputs de fecha, `from`/`to`).
   - `Categoría` (select, incluye opción `Todas` + lista de
     `GET /api/categories`, cada opción con el punto de color de
     `CategoryBadge` `sm`).
   - Botón fantasma `Limpiar filtros` (visible solo si hay algún filtro
     activo).
   - Los filtros aplican de inmediato al cambiar (sin botón "Aplicar"
     adicional) y actualizan la URL (querystring) para que el estado sea
     compartible/recargable.
3. **Lista/tabla de gastos** (§3.4) dentro de una tarjeta contenedora, con
   paginación al pie (`page`/`pageSize`/`totalItems` de la respuesta).
4. Estado vacío (§3.9) cuando `items.length === 0`, dentro del mismo
   contenedor que reemplazaría la tabla.
5. Carga: skeleton de 5 filas (`h-16` móvil / `h-12` filas de tabla en
   escritorio).

**Alta/edición de gasto** — modal (§3.6), formulario `ExpenseForm` en
`space-y-4`:

- `Monto` (número, prefijo visual de moneda no necesario al ser moneda
  única — placeholder `0.00`, `inputmode="decimal"`).
- `Fecha` (date, máximo hoy — coherente con la validación de negocio de "no
  fecha futura"; el input nativo `max` lo refuerza en cliente además de
  mostrar el error `400` de la API si igualmente ocurre).
- `Categoría` (select, mismas opciones que el filtro, sin la opción `Todas`).
- `Nota` (textarea opcional, contador de caracteres `mt-1 text-xs text-ink-muted text-right`
  cuando se acerca al límite de 500).
- Pie del modal: `Cancelar` + `Guardar gasto` (primario, estado de carga
  mientras `POST`/`PUT`); al editar, título del modal `Editar gasto` y botón
  `Guardar cambios`.
- Errores de validación de API (`400`) se mapean campo a campo (§3.2); error
  genérico de categoría inaccesible (`404`) se muestra como banner superior
  del formulario.

**Borrado de gasto**: modal de confirmación (§3.6) con texto `¿Eliminar este
gasto? Esta acción no se puede deshacer.` + botón peligro `Eliminar`.

### 4.5 Categorías (`/categories`)

Propósito: ver todas las categorías disponibles (predefinidas + propias) y
gestionar (crear/editar/borrar) las categorías personalizadas del usuario.

Layout:

1. `H1`: `Categorías` + botón primario `+ Crear categoría` a la derecha en
   `md:` (ancho completo debajo del título en móvil).
2. **Lista de categorías** en grilla de tarjetas pequeñas,
   `grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3`, agrupada en dos
   secciones con encabezado `H4` (`Predefinidas`, `Tus categorías`):
   - Cada tarjeta: `flex items-center justify-between gap-3 rounded-lg border border-line bg-surface-raised px-4 py-3`
     — `CategoryBadge` `md` a la izquierda (punto + nombre); a la derecha,
     para categorías propias, botones fantasma icon-only `Editar`/`Eliminar`
     (§3.4, mismo patrón que la tabla de gastos); las predefinidas no llevan
     acciones (no son editables ni borrables — en su lugar, opcionalmente el
     texto `Predefinida` en `text-xs text-ink-muted`).
3. Estado vacío (§3.9) solo para la sección `Tus categorías` cuando el
   usuario no tiene ninguna propia (las predefinidas siempre existen por
   seed, así que la vista nunca está vacía por completo).
4. Carga: skeleton de 6 tarjetas (`h-14`) en la grilla.

**Alta/edición de categoría** — modal (§3.6), formulario `CategoryForm`:

- `Nombre` (texto, 1–60 caracteres).
- `Color` (selector de 8 círculos de la paleta categórica, §3.2).
- `Icono` (opcional; si se implementa, select simple de un set reducido de
  nombres de icono — fuera del detalle visual de este documento al no tener
  un set de iconos de categoría definido en el contrato de API más allá del
  string libre; el frontend puede omitir este campo en el MVP sin romper el
  contrato, ya que `icon` es opcional).
- Pie: `Cancelar` + `Guardar categoría` / `Guardar cambios`.
- Conflicto de nombre duplicado (`409`): error de campo en `Nombre`
  (`Ya tenés una categoría con ese nombre.`).

**Borrado de categoría**: modal de confirmación estándar
(`¿Eliminar esta categoría? Esta acción no se puede deshacer.` + `Eliminar`
peligro); si la API responde `409` (gastos asociados), el modal cambia al
estado de aviso descrito en §3.6 en vez de cerrarse.

### 4.6 Nota sobre cobertura de vistas

ARCHITECTURE.md enumera las vistas del frontend de forma ilustrativa como
`views/ # páginas: Login, Register, Dashboard, Expenses`, sin mencionar
explícitamente una pantalla de gestión de categorías. Sin embargo, el
contrato de API (§7.3) define CRUD completo de categorías personalizadas
(crear, editar, borrar, con manejo de conflicto `409`), y el alcance del MVP
lista "categorías predefinidas + personalizadas" como funcionalidad propia.
Ese CRUD necesita una superficie de UI: este documento la resuelve como una
vista dedicada `/categories` (§4.5) en vez de forzarlo dentro del formulario
de gasto. Se señala aquí para que arquitectura confirme o ajuste la lista de
rutas/vistas si se prefiere otro enfoque (p. ej. un panel dentro de
`Configuración` en una fase posterior).

### 4.7 Presupuestos (`/budgets`)

Propósito: fijar un límite mensual de gasto por categoría y seguir el progreso
del mes en curso. Ruta protegida `/budgets`, cuarto destino de la navegación
principal (§3.7: sidebar en escritorio, tab bar en móvil), con icono `wallet` (o
`target`) y etiqueta `Presupuestos`.

Layout (§4.1): `H1` `Presupuestos` + botón primario `+ Crear presupuesto` a la
derecha en `md:` (ancho completo bajo el título en móvil), seguido de
`space-y-6`.

Cuerpo:

1. **Lista de presupuestos**, una tarjeta (§3.3) por presupuesto, `space-y-3` o
   grilla `grid gap-3 sm:grid-cols-2`. Cada tarjeta contiene un `BudgetProgress`
   (§3.11) y, alineados a la derecha de la cabecera o en un pie, botones fantasma
   icon-only `Editar` (`edit`) y `Eliminar` (`trash`) compactos (mismo patrón que
   la tabla de gastos, §3.4).
2. **Estado vacío** (§3.9) cuando el usuario no tiene presupuestos: icono
   `wallet`, texto `Todavía no tienes presupuestos. Crea uno para controlar tu
   gasto mensual por categoría.` + botón `Crear presupuesto`.
3. **Carga**: skeleton de 3 tarjetas (`h-24`). **Error de vista** (§3.9) con
   `Reintentar`.

**Alta/edición** — modal (§3.6), formulario `BudgetForm` `space-y-4`:

- `Categoría` (select, §3.2): en **alta**, lista de categorías del usuario
  (globales + propias) que **aún no tienen** presupuesto (cada opción con el punto
  de color, §3.5); en **edición**, la categoría se muestra fija (deshabilitada),
  porque la categoría es la identidad del presupuesto.
- `Límite mensual` (número, `inputmode="decimal"`, placeholder `0.00`, prefijo de
  moneda no necesario al ser moneda única), ayuda `El importe máximo que quieres
  gastar en esta categoría cada mes.`
- Pie: `Cancelar` + `Guardar presupuesto` / `Guardar cambios` (primario, estado
  de carga mientras `POST`/`PUT`).
- Error `409` (ya existe presupuesto para la categoría): error de campo en
  `Categoría` (`Ya tienes un presupuesto para esta categoría.`). Error `400`
  (importe inválido): error de campo en `Límite mensual`.

**Borrado**: modal de confirmación estándar (§3.6) `¿Eliminar este presupuesto?
El gasto registrado no se ve afectado.` + botón peligro `Eliminar`.

**Sección en el Dashboard (§4.3).** Cuando el usuario tiene al menos un
presupuesto, el Dashboard muestra, bajo la tarjeta de total, una tarjeta
`Presupuestos del mes` con hasta los presupuestos ordenados por porcentaje
descendente (los más ajustados primero), cada uno como un `BudgetProgress`
(§3.11), y un enlace `Gestionar presupuestos` a `/budgets`. Si no hay
presupuestos, la sección no aparece (no añade ruido a quien no los usa). Los
datos salen de `GET /api/budgets`; su carga es independiente del
`summary` y muestra su propio skeleton (`h-24`).

---

## 5. Accesibilidad

- **Contraste**: todos los pares texto/fondo de la paleta (§1.3) cumplen
  WCAG AA (≥ 4.5:1 texto normal, ≥ 3:1 texto grande/iconos), verificado en
  ambos modos. `warning` como texto se reserva para tamaño ≥ 18px/semibold o
  acompañado de icono, por su contraste más ajustado sobre `surface` claro.
- **Foco visible**: regla global única `outline-2 outline-offset-2
  outline-primary` en `:focus-visible` (§2); el botón peligro usa
  `outline-danger` para que el foco combine con la acción. Nunca
  `outline-none` sin reemplazo equivalente.
- **Objetivos táctiles**: mínimo 44×44px en todo elemento interactivo
  (botones `h-11`, filas de tabla clicables, ítems de tab bar `h-16`, avatar
  `size-9` con área de toque ampliada por padding del botón contenedor). Todo
  botón icon-only usa por defecto `size-11` (44px) con icono `size-6` (24px,
  §3.1); la única excepción es la variante compacta (`size-8`/icono `size-4`)
  de las acciones de fila en tablas de escritorio, documentada explícitamente
  como tal en §3.1 y §3.4. Los iconos que acompañan a un texto (filas del menú
  de usuario, enlaces de nav, tab bar) son `size-5` (20px), no icon-only.
- **Conmutador de tema accesible**: `ThemeToggle` (§3.10) es una fila del menú
  de usuario (§3.7) con texto visible (`Modo claro`/`Modo oscuro`), foco
  visible y objetivo táctil `h-11` (44px); el estado inicial respeta
  `prefers-color-scheme` y no hay parpadeo de tema incorrecto en la carga
  (§1.2); el cambio nunca depende solo del color: el icono (sol/luna) y el
  texto cambian junto con el tema.
- **El color nunca es el único indicador**: estado de navegación activo
  (color + peso de fuente + `aria-current`), badges de categoría (color +
  nombre siempre visible), toasts (color + icono distinto por tipo + texto),
  campos inválidos (color + icono + mensaje de texto + `aria-invalid`),
  estado seleccionado del selector de color de categoría (color + check +
  `aria-checked`).
- **Roles y regiones vivas**: `role="alert"` en errores y mensajes de campo
  inválido; `role="status"` en spinners, skeletons y toasts de
  éxito/info; `role="dialog"` + `aria-modal="true"` + `aria-labelledby` en
  modales, con trampa de foco y devolución de foco al cerrar.
- **Navegación por teclado**: orden de tabulación natural (DOM = orden
  visual); `Escape` cierra modales; `Enter`/`Space` activan botones y
  opciones de radiogroup del selector de color.
- **Idioma**: `<html lang="es">`; mensajes de error de la API ya vienen en
  español y se muestran tal cual.
- **Movimiento**: transiciones cortas (`duration-150`–`200`) en color/sombra;
  skeletons con `animate-pulse motion-reduce:animate-none`; sin animaciones
  de entrada/salida decorativas en listas o tarjetas.
