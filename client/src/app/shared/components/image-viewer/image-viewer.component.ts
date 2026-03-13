import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-image-viewer',
  standalone: true,
  imports: [CommonModule, DialogModule, ButtonModule],
  templateUrl: './image-viewer.component.html',
  styleUrls: ['./image-viewer.component.css']
})
export class ImageViewerComponent {
  @Input() imageUrl: string = '';
  visible: boolean = false;

  open(imageUrl: string) {
    this.imageUrl = imageUrl;
    this.visible = true;
  }

  close() {
    this.visible = false;
  }
}
