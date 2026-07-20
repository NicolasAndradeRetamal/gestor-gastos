import { onBeforeUnmount, onMounted, ref, type Ref } from 'vue'

// Resolves a `--color-*` token (declared with `light-dark()`) to a concrete
// rgb() value for the active theme. A <canvas> fillStyle can't parse
// var()/light-dark(), so we read the token back off a probe element whose
// computed `color` the browser has already resolved. Re-reads when the theme
// changes, whether from the manual toggle (`data-theme`) or the OS preference.
export function useCssColor(variableName: string): Ref<string> {
  const value = ref('')

  function read(): void {
    const probe = document.createElement('span')
    probe.style.cssText = `color: var(${variableName}); position: absolute; opacity: 0; pointer-events: none`
    document.body.appendChild(probe)
    value.value = getComputedStyle(probe).color
    probe.remove()
  }

  const media = window.matchMedia?.('(prefers-color-scheme: dark)')
  const observer = new MutationObserver(read)

  onMounted(() => {
    read()
    media?.addEventListener('change', read)
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] })
  })
  onBeforeUnmount(() => {
    media?.removeEventListener('change', read)
    observer.disconnect()
  })

  return value
}
