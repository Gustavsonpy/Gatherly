import { Component, computed } from '@angular/core';
import { PurpleButton } from '../buttons/purple-button';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [PurpleButton],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  constructor(private readonly authService: AuthService){}

  transformUsername(completeName: string): string {
    const nameSeparated: string[] = completeName.split(" ");
    const nameLength = nameSeparated.length;

    console.log(nameLength);

    return nameSeparated[0] + " " + nameSeparated[nameLength-1];
  }

  readonly username = computed(() => {
    return this.transformUsername(this.authService.currentUser()?.name ?? "Não foi");
  });
}
