import { Component, computed, OnInit, signal} from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { GenericButton } from '../buttons/generic-button';
import { AuthService } from '../../core/auth/auth.service';
import { UserService } from '../../core/user/user.service';

@Component({
  selector: 'app-sidebar',
  imports: [GenericButton, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
  host: {
    // Fecha o menu do celular com a tecla Esc.
    '(document:keydown.escape)': 'closeMenu()',
  },
})
export class Sidebar implements OnInit {
  constructor(private readonly authService: AuthService, private readonly userService: UserService){}

  menuOpen = signal(false);

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  usernamee = signal<string>('');
  userPhoto = signal<string | null>('');

  ngOnInit(): void {
    this.userService.getMe().subscribe({
      next: (user) => {
        this.usernamee.set(this.transformUsername(user.name));
        this.userPhoto.set(user.urlPhoto)
      },
      // Se a API falhar (ex.: token expirado), mostra o nome que já está no token
      error: () => this.usernamee.set(this.username()),
    });

  }

  transformUsername(completeName: string): string {
    const nameSeparated: string[] = completeName.split(" ");
    const nameLength = nameSeparated.length;

    return nameSeparated[0] + " " + nameSeparated[nameLength-1];
  }

  readonly isAdmin = computed(() => this.authService.isAdmin());

  readonly username = computed(() => {
    return this.transformUsername(this.authService.currentUser()?.name ?? "Não existente");
  });


}