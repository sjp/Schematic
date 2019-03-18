'use strict';

const gulp = require('gulp');
const newer = require('gulp-newer');
const uglify = require('gulp-uglify');
const concat = require('gulp-concat');
const cssnano = require('gulp-cssnano');
const postcss = require('gulp-postcss');
const cssnext = require('postcss-cssnext');
const del = require('del');

gulp.task('clean', function () {
    return del([
        'assets/**/*.*'
    ]);
});

gulp.task('source-sans-font', function () {
    return gulp.src('node_modules/source-sans-pro/**/*.{eot,woff,woff2,otf,ttf,svg}')
        .pipe(newer('assets/fonts'))
        .pipe(gulp.dest('assets/fonts'));
});

gulp.task('fontawesome-font', function () {
    return gulp.src('node_modules/font-awesome/fonts/*-webfont.{eot,woff,woff2,otf,ttf,svg}')
        .pipe(newer('assets/fonts'))
        .pipe(gulp.dest('assets/fonts'));
});

gulp.task('glyphicons-font', function () {
    return gulp.src('node_modules/bootstrap/dist/fonts/*.{eot,woff,woff2,otf,ttf,svg}')
        .pipe(newer('assets/fonts'))
        .pipe(gulp.dest('assets/fonts'));
});

gulp.task('fonts', ['source-sans-font', 'fontawesome-font', 'glyphicons-font'], function () {
});

gulp.task('copy-assets', ['fonts'], function () {
});

gulp.task('styles:dev', ['copy-assets'], function () {
    return gulp.src(
        [
            'node_modules/bootstrap/dist/css/bootstrap.css',
            'node_modules/font-awesome/css/font-awesome.css',
            'node_modules/ionicons/dist/css/ionicons.css',
            'node_modules/datatables.net-bs4/css/dataTables.bootstrap4.css',
            'node_modules/codemirror/lib/codemirror.css',
            'node_modules/codemirror/theme/material.css',
            'node_modules/admin-lte/dist/css/adminlte.css',
            'node_modules/admin-lte/dist/css/skins/_all-skins.css',
            'Source/css/source-sans-pro.css',
            'Source/css/reporting.css'
        ])
        .pipe(newer('assets/css/reporting-app.css'))
        .pipe(concat('reporting-app.css'))
        .pipe(postcss([cssnext]))
        .pipe(gulp.dest('assets/'));
});

gulp.task('scripts:dev', function () {
    return gulp.src(
        [
            'node_modules/jquery/dist/jquery.js',
            'node_modules/jquery-ui-dist/jquery-ui.js',
            'node_modules/bootstrap/dist/js/bootstrap.js',
            'node_modules/datatables.net/js/jquery.dataTables.js',
            'node_modules/datatables.net-bs4/js/dataTables.bootstrap4.js',
            'node_modules/salvattore/dist/salvattore.js',
            'node_modules/codemirror/lib/codemirror.js',
            'node_modules/codemirror/mode/sql/sql.js',
            'node_modules/admin-lte/dist/js/adminlte.js',
            'node_modules/svg-pan-zoom/dist/svg-pan-zoom.js',
            'Source/js/reporting.js'
        ])
        .pipe(newer('assets/js/reporting-app.js'))
        .pipe(concat('reporting-app.js'))
        .pipe(eslint())
        .pipe(gulp.dest('assets/'));
});

gulp.task('build:dev', ['styles:dev', 'scripts:dev'], function () {

});

gulp.task('styles:prod', ['copy-assets'], function () {
    return gulp.src(
        [
            'node_modules/bootstrap/dist/css/bootstrap.min.css',
            'node_modules/font-awesome/css/font-awesome.min.css',
            'node_modules/ionicons/dist/css/ionicons.min.css',
            'node_modules/datatables.net-bs4/css/dataTables.bootstrap4.css',
            'node_modules/codemirror/lib/codemirror.css',
            'node_modules/codemirror/theme/material.css',
            'node_modules/admin-lte/dist/css/adminlte.min.css',
            'node_modules/admin-lte/dist/css/skins/_all-skins.min.css',
            'Source/css/source-sans-pro.css',
            'Source/css/reporting.css'
        ])
        .pipe(newer('assets/css/reporting-app.css'))
        .pipe(concat('reporting-app.css'))
        .pipe(postcss([cssnext]))
        .pipe(cssnano())
        .pipe(gulp.dest('assets/css'));
});

gulp.task('scripts:prod', function () {
    return gulp.src(
        [
            'node_modules/jquery/dist/jquery.min.js',
            'node_modules/jquery-ui-dist/jquery-ui.min.js',
            'node_modules/bootstrap/dist/js/bootstrap.min.js',
            'node_modules/datatables.net/js/jquery.dataTables.min.js',
            'node_modules/datatables.net-bs4/js/dataTables.bootstrap4.min.js',
            'node_modules/salvattore/dist/salvattore.min.js',
            'node_modules/anchor-js/anchor.min.js',
            'node_modules/codemirror/lib/codemirror.js',
            'node_modules/codemirror/mode/sql/sql.js',
            'node_modules/admin-lte/dist/js/adminlte.min.js',
            'node_modules/svg-pan-zoom/dist/svg-pan-zoom.min.js',
            'Source/js/reporting.js'
        ])
        .pipe(newer('assets/js/reporting-app.js'))
        .pipe(concat('reporting-app.js'))
        .pipe(uglify())
        .pipe(gulp.dest('assets/js'));
});

gulp.task('build:prod', ['styles:prod', 'scripts:prod'], function () {

});

gulp.task('default', ['build:prod'], function () {

});