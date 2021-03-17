import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'semicolonToNewLine'
})
export class SemicolonToNewLinePipe implements PipeTransform {

  transform(value: string): string {
    return value.replace(/;/g, "\n");
  }

}
