import gulp from 'gulp';
const { src, dest, parallel } = gulp;
import gzip from 'gulp-gzip';
import newer from 'gulp-newer';
import terser from 'gulp-terser';
import concat from 'gulp-concat';
import cssnano from 'cssnano';
import postcss from 'gulp-postcss';
import postcssPresetEnv from 'postcss-preset-env';
import { deleteAsync } from 'del';

const cssNanoPreset = {
    preset: ['default', {
        discardComments: {
            removeAll: true,
        },
    }]
};

export const clean = () =>
    deleteAsync([
        'assets/**/*.*'
    ]);

const sourceSansFont = () => {
    return src('node_modules/source-sans/WOFF/OTF/*.otf.woff')
        .pipe(newer('assets/fonts'))
        .pipe(dest('assets/fonts'));
}

const fontAwesomeFont = () => {
    return src('node_modules/font-awesome/fonts/*-webfont.woff')
        .pipe(newer('assets/fonts'))
        .pipe(dest('assets/fonts'));
}

const fonts = parallel(sourceSansFont, fontAwesomeFont);

const stylesDev = () => {
    return src(
        [
            'node_modules/bootstrap/dist/css/bootstrap.css',
            'node_modules/datatables.net-bs4/css/dataTables.bootstrap4.css',
            'node_modules/codemirror/lib/codemirror.css',
            'node_modules/codemirror/theme/material.css',
            'node_modules/admin-lte/dist/css/adminlte.css',
            'Source/css/font-awesome.css',
            'Source/css/source-sans-pro.css',
            'Source/css/reporting.css'
        ])
        .pipe(newer('assets/css/reporting-app.css'))
        .pipe(concat('reporting-app.css'))
        .pipe(postcss([postcssPresetEnv()]))
        .pipe(dest('assets/'));
}

const scriptsDev = () => {
    return src(
        [
            'node_modules/jquery/dist/jquery.js',
            'node_modules/jquery-ui-dist/jquery-ui.js',
            'node_modules/bootstrap/dist/js/bootstrap.js',
            'node_modules/datatables.net/js/jquery.dataTables.js',
            'node_modules/datatables.net-bs4/js/dataTables.bootstrap4.js',
            'node_modules/codemirror/lib/codemirror.js',
            'node_modules/codemirror/mode/sql/sql.js',
            'node_modules/admin-lte/dist/js/adminlte.js',
            'node_modules/hammerjs/hammer.js',
            'node_modules/svg-pan-zoom/dist/svg-pan-zoom.js',
            'Source/js/reporting.js'
        ])
        .pipe(newer('assets/js/reporting-app.js'))
        .pipe(concat('reporting-app.js'))
        .pipe(eslint())
        .pipe(dest('assets/'));
}

const stylesProd = () => {
    return src(
        [
            'node_modules/bootstrap/dist/css/bootstrap.min.css',
            'node_modules/datatables.net-bs4/css/dataTables.bootstrap4.css',
            'node_modules/codemirror/lib/codemirror.css',
            'node_modules/codemirror/theme/material.css',
            'node_modules/admin-lte/dist/css/adminlte.min.css',
            'Source/css/font-awesome.css',
            'Source/css/source-sans-pro.css',
            'Source/css/reporting.css'
        ])
        .pipe(newer('assets/css/reporting-app.css'))
        .pipe(concat('reporting-app.css'))
        .pipe(postcss([postcssPresetEnv(), cssnano(cssNanoPreset)]))
        .pipe(gzip({ append: true, gzipOptions: { level: 9 } }))
        .pipe(dest('assets/css'));
}

const scriptsProd = () => {
    return src(
        [
            'node_modules/jquery/dist/jquery.min.js',
            'node_modules/jquery-ui-dist/jquery-ui.min.js',
            'node_modules/bootstrap/dist/js/bootstrap.min.js',
            'node_modules/datatables.net/js/jquery.dataTables.min.js',
            'node_modules/datatables.net-bs4/js/dataTables.bootstrap4.min.js',
            'node_modules/codemirror/lib/codemirror.js',
            'node_modules/codemirror/mode/sql/sql.js',
            'node_modules/admin-lte/dist/js/adminlte.min.js',
            'node_modules/hammerjs/hammer.min.js',
            'node_modules/svg-pan-zoom/dist/svg-pan-zoom.min.js',
            'Source/js/reporting.js'
        ])
        .pipe(newer('assets/js/reporting-app.js'))
        .pipe(concat('reporting-app.js'))
        .pipe(terser())
        .pipe(gzip({ append: true, gzipOptions: { level: 9 } }))
        .pipe(dest('assets/js'));
}

export const buildDev = parallel(fonts, scriptsDev, stylesDev);

export const buildProd = parallel(fonts, scriptsProd, stylesProd);

export default buildProd;
