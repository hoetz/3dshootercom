var gulp = require('gulp');
var concat = require('gulp-concat');
var watch = require('gulp-watch');

var config = {
    //Include all js files but exclude any min.js files
    src: [
            'bower_components/jquery/dist/jquery.js',
            'bower_components/handlebars/handlebars.js',
            'bower_components/bootstrap/dist/js/bootstrap.js',
            'scripts/*.js',
            'scripts/**/*.js'
    ],
    rootFolder:"wwwroot/dist/app"
}

gulp.task('default', ['scripts', 'BootstrapFontCss'], function () {

});


gulp.task('scripts', function () {
    return gulp.src(config.src
        ,
        { base: 'bower_components/' })
      .pipe(concat('main.js'))
      .pipe(gulp.dest(config.rootFolder + '/js'));
});


gulp.task('BootstrapFontCss', function () {
    gulp.src('bower_components/bootstrap/dist/fonts/**/*.{ttf,woff,woff2,eof,eot,svg}')
    .pipe(gulp.dest(config.rootFolder+'/fonts'));

    gulp.src(['bower_components/bootstrap/dist/css/*.min.css','Content/stylesheets/*.css'])
    .pipe(gulp.dest(config.rootFolder+'/css'));
});

gulp.task('watch', function () {
    //gulp.start('aspnet-run');
    gulp.watch([config.src,config.templates], ['scripts','templates']);
});