import { test, expect } from '@playwright/test';

test.use({
    storageState: 'auth.json'
});

test('test', async ({ page }) => {
    await page.goto('http://localhost:5176/');
    await page.getByRole('link', { name: 'Zarządzaj Projektami' }).click();
    await page.getByTitle('Edytuj').first().click();
    await page.getByRole('button', { name: ' Usuń' }).first().click();
    await page.getByRole('button', { name: ' Zapisz zmiany' }).click();
    await page.getByRole('link', { name: 'Anuluj' }).click();
});