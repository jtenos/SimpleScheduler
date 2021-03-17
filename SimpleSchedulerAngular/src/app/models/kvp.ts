export default class Kvp<T1, T2> {
    key: T1;
    value: T2;

    constructor(key: T1, value: T2) {
        this.key = key;
        this.value = value;
    }
}
