import { Component, ElementRef, OnDestroy, computed, effect, input, viewChild } from '@angular/core';
import * as L from 'leaflet';

/**
 * Mapa estático e pequeno (Leaflet + tiles OpenStreetMap, sem API key).
 * Clicar abre o local no Google Maps em outra aba.
 */
@Component({
    selector: 'mini-map',
    template: `
        <a
            [href]="externalUrl()"
            target="_blank"
            rel="noopener"
            class="relative block w-full h-full rounded-md overflow-hidden bg-gray-100"
            title="Abrir no Google Maps">
            <div #map class="w-full h-full pointer-events-none"></div>
        </a>
    `,
    host: { class: 'block' },
    styles: `
        :host ::ng-deep .leaflet-control-attribution {
            font-size: 7px;
            line-height: 1.4;
            padding: 0 3px;
            background: rgba(255, 255, 255, 0.7);
        }
    `,
})
export class MiniMap implements OnDestroy {
    latitude = input.required<number>();
    longitude = input.required<number>();
    zoom = input(16);

    private readonly mapEl = viewChild.required<ElementRef<HTMLDivElement>>('map');
    private map?: L.Map;
    private marker?: L.CircleMarker;
    private resizeObserver?: ResizeObserver;

    readonly externalUrl = computed(
        () => `https://www.google.com/maps/search/?api=1&query=${this.latitude()},${this.longitude()}`,
    );

    constructor() {
        effect(() => {
            const center: L.LatLngExpression = [this.latitude(), this.longitude()];

            if (!this.map) {
                this.map = L.map(this.mapEl().nativeElement, {
                    center,
                    zoom: this.zoom(),
                    zoomControl: false,
                    dragging: false,
                    scrollWheelZoom: false,
                    doubleClickZoom: false,
                    boxZoom: false,
                    keyboard: false,
                    touchZoom: false,
                    attributionControl: true,
                });
                this.map.attributionControl.setPrefix(false);

                // Tiles públicos do OSM: grátis, sem chave. Exige o crédito visível.
                // Política de uso: https://operations.osmfoundation.org/policies/tiles/
                L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '© OpenStreetMap',
                }).addTo(this.map);

                // circleMarker é desenhado em SVG: evita o problema dos ícones PNG do Leaflet com o bundler
                this.marker = L.circleMarker(center, {
                    radius: 7,
                    color: '#ffffff',
                    weight: 2,
                    fillColor: '#7C3AED',
                    fillOpacity: 1,
                }).addTo(this.map);

                // Leaflet calcula o tamanho só na criação; se o container ainda estava 0x0
                // (layout não pronto, aba escondida etc.), recalcula quando ganhar tamanho.
                this.resizeObserver = new ResizeObserver(() => this.map?.invalidateSize());
                this.resizeObserver.observe(this.mapEl().nativeElement);
            } else {
                this.map.setView(center, this.zoom());
                this.marker?.setLatLng(center);
            }
        });
    }

    ngOnDestroy(): void {
        this.resizeObserver?.disconnect();
        this.map?.remove();
    }
}
