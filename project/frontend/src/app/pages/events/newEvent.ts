import { Component } from '@angular/core';
import { Sidebar } from '../../components/sidebar/sidebar';
import { GenericInput } from '../../components/input/generic-input/generic-input';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { GenericButton } from '../../components/buttons/generic-button';

interface NewEventForm {
    eventName: FormControl<string>;
    category: FormControl<string>;
    description: FormControl<string>;
    date: FormControl<string>;
    time: FormControl<string>;
    localization: FormControl<string>;
    spots: FormControl<number>;
    level: FormControl<string>;
    imagemURl: FormControl<string>;
}

@Component({
  selector: 'app-new-event',
  standalone: true,
  imports: [Sidebar, GenericInput, GenericButton],
  templateUrl: 'newEvent.html',
})
export class NewEvent {
    readonly form = new FormGroup<NewEventForm>({
        eventName: new FormControl('', {
            nonNullable: true,
            validators: [Validators.required]
        }),
        category: new FormControl('', {
            nonNullable: true,
            validators: [Validators.required]
        }),
        description: new FormControl('', {
            nonNullable: true,
            validators: [Validators.required]
        }),
        date: new FormControl('', {
            nonNullable: true,
            validators: [Validators.required]
        }),
        time: new FormControl('', {
            nonNullable: true,
            validators: [Validators.required]
        }),
        localization: new FormControl('', {
            nonNullable: true,
            validators: [Validators.required]
        }),
        spots: new FormControl(0, {
            nonNullable: true,
            validators: [Validators.required, Validators.min(1)]
        }),
        level: new FormControl('', {
            nonNullable: true
        }),
        imagemURl: new FormControl('', {
            nonNullable: true,
            validators: [Validators.required]
        }),
    })

    
}